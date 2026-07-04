using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IdleRepairTycoon
{
    public sealed class IdleGameController : MonoBehaviour
    {
        public event Action OnStateChanged;
        public event Action<double, bool> OnOfflineIncomeCalculated;
        public event Action<string> OnToast;

        public IdleGameSaveData Save { get; private set; }
        public IReadOnlyList<StationRuntime> Stations => stations;
        public string SelectedStationId { get; private set; }
        public StationRuntime SelectedStation => GetStation(SelectedStationId) ?? stations.FirstOrDefault();
        public double CurrentIncomePerSecond => CalculateIncomePerSecond();
        public bool HasActiveBoost => IdleGameBalance.UnixNow() < Save.BoostUntilUnix;
        public int BoostSecondsRemaining => Mathf.Max(0, (int)(Save.BoostUntilUnix - IdleGameBalance.UnixNow()));
        public double PrestigeMultiplier => 1d + (Save.ReputationStars * 0.12d);
        public double BoostMultiplier => HasActiveBoost ? IdleGameBalance.RewardedBoostMultiplier : 1d;
        public bool CanPrestige => Save.Cash >= IdleGameBalance.PrestigeCost;

        private readonly List<StationRuntime> stations = new List<StationRuntime>();
        private IIdleAdsService ads;
        private float saveTimer;
        private float uiTickTimer;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            ads = IdleAdsFactory.Create();
            ads.Initialize();
            LoadOrCreateSave();
        }

        private void Start()
        {
            var world = gameObject.AddComponent<IdleGameWorld>();
            world.Bind(this);

            var ui = gameObject.AddComponent<IdleGameUI>();
            ui.Bind(this);

            CalculateOfflineIncome();
            RaiseStateChanged();
        }

        private void Update()
        {
            TickProduction(Time.deltaTime);

            saveTimer += Time.deltaTime;
            uiTickTimer += Time.deltaTime;

            if (uiTickTimer >= 0.15f)
            {
                uiTickTimer = 0;
                RaiseStateChanged();
            }

            if (saveTimer >= 5f)
            {
                saveTimer = 0;
                SaveGame();
            }
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause) SaveGame();
            else
            {
                LoadOrCreateSave();
                CalculateOfflineIncome();
                RaiseStateChanged();
            }
        }

        private void OnApplicationQuit()
        {
            SaveGame();
        }

        public void SelectStation(string stationId)
        {
            StationRuntime station = FindStation(stationId);
            if (station == null) return;

            SelectedStationId = stationId;
            Toast(station.Definition.Title + " selecionado.");
            RaiseStateChanged();
        }

        public StationRuntime GetStation(string stationId)
        {
            if (string.IsNullOrEmpty(stationId)) return null;
            return FindStation(stationId);
        }

        public void UnlockStation(string stationId)
        {
            StationRuntime station = FindStation(stationId);
            if (station == null || station.Save.Unlocked) return;

            if (Save.ReputationStars < station.Definition.RequiredReputation)
            {
                Toast("Precisa de reputação " + station.Definition.RequiredReputation + " para liberar.");
                return;
            }

            if (!Spend(station.Definition.UnlockCost))
            {
                Toast("Dinheiro insuficiente.");
                return;
            }

            station.Save.Unlocked = true;
            station.Save.Level = Mathf.Max(1, station.Save.Level);
            station.Save.Progress = 0;
            SelectedStationId = station.Definition.Id;
            Toast(station.Definition.Title + " liberado!");
            SaveGame();
            RaiseStateChanged();
        }

        public void UpgradeStation(string stationId)
        {
            StationRuntime station = FindStation(stationId);
            if (station == null || !station.Save.Unlocked) return;

            double cost = station.UpgradeCost();
            if (!Spend(cost))
            {
                Toast("Dinheiro insuficiente para melhorar.");
                return;
            }

            station.Save.Level++;
            SelectedStationId = station.Definition.Id;
            Toast(station.Definition.Title + " nível " + station.Save.Level + "!");
            SaveGame();
            RaiseStateChanged();
        }

        public void WatchAdForBoost()
        {
            ads.ShowRewarded(
                onRewardEarned: () =>
                {
                    long now = IdleGameBalance.UnixNow();
                    long baseStart = Math.Max(now, Save.BoostUntilUnix);
                    Save.BoostUntilUnix = baseStart + IdleGameBalance.RewardedBoostSeconds;
                    Toast("Turbo 2x ativado por 5 minutos.");
                    SaveGame();
                    RaiseStateChanged();
                },
                onFailed: error => Toast(error)
            );
        }

        public void WatchAdForMoneyPackage()
        {
            ads.ShowRewarded(
                onRewardEarned: () =>
                {
                    double reward = Math.Max(120d, CalculateIncomePerSecond() * IdleGameBalance.PackageRewardSeconds);
                    AddCash(reward);
                    Toast("Pacote recebido: " + IdleGameBalance.FormatMoney(reward));
                    SaveGame();
                    RaiseStateChanged();
                },
                onFailed: error => Toast(error)
            );
        }

        public void DoubleOfflineIncomeWithAd(double offlineIncome)
        {
            if (offlineIncome <= 0) return;

            ads.ShowRewarded(
                onRewardEarned: () =>
                {
                    AddCash(offlineIncome);
                    Toast("Ganho offline dobrado!");
                    SaveGame();
                    RaiseStateChanged();
                },
                onFailed: error => Toast(error)
            );
        }

        public void Prestige()
        {
            if (!CanPrestige)
            {
                Toast("Precisa de " + IdleGameBalance.FormatMoney(IdleGameBalance.PrestigeCost) + ".");
                return;
            }

            Save.ReputationStars++;
            Save.Cash = IdleGameBalance.StartingCash;
            Save.BoostUntilUnix = 0;
            Save.Stations.Clear();
            BuildStationRuntimeFromDefinitions();
            Toast("Prestígio feito! Multiplicador permanente aumentado.");
            SaveGame();
            RaiseStateChanged();
        }

        public void ResetSaveForTesting()
        {
            PlayerPrefs.DeleteKey(IdleGameBalance.SaveKey);
            PlayerPrefs.Save();
            LoadOrCreateSave(true);
            Toast("Save resetado.");
            RaiseStateChanged();
        }

        private void TickProduction(float deltaTime)
        {
            foreach (StationRuntime station in stations)
            {
                if (!station.Save.Unlocked) continue;

                station.Save.Progress += deltaTime;
                float duration = station.DurationSeconds();

                while (station.Save.Progress >= duration)
                {
                    station.Save.Progress -= duration;
                    AddCash(station.ProfitPerJob(PrestigeMultiplier, BoostMultiplier));
                }
            }
        }

        private void CalculateOfflineIncome()
        {
            long now = IdleGameBalance.UnixNow();
            if (Save.LastSaveUnix <= 0 || Save.LastSaveUnix >= now) return;

            int elapsed = Mathf.Clamp((int)(now - Save.LastSaveUnix), 0, IdleGameBalance.MaxOfflineSeconds);
            if (elapsed < 10) return;

            double income = CalculateIncomePerSecond(ignoreBoost: true) * elapsed * IdleGameBalance.OfflineIncomeRate;
            if (income <= 0) return;

            AddCash(income);
            OnOfflineIncomeCalculated?.Invoke(income, elapsed >= IdleGameBalance.MaxOfflineSeconds);
            SaveGame();
        }

        private void LoadOrCreateSave(bool forceNew = false)
        {
            if (!forceNew && PlayerPrefs.HasKey(IdleGameBalance.SaveKey))
            {
                try
                {
                    Save = UnityEngine.JsonUtility.FromJson<IdleGameSaveData>(PlayerPrefs.GetString(IdleGameBalance.SaveKey));
                }
                catch
                {
                    Save = null;
                }
            }

            if (Save == null)
            {
                Save = new IdleGameSaveData
                {
                    Cash = IdleGameBalance.StartingCash,
                    LastSaveUnix = IdleGameBalance.UnixNow()
                };
            }

            if (Save.Stations == null) Save.Stations = new List<StationSaveData>();
            BuildStationRuntimeFromDefinitions();
        }

        private void BuildStationRuntimeFromDefinitions()
        {
            stations.Clear();

            foreach (StationDefinition definition in IdleGameBalance.StationDefinitions)
            {
                StationSaveData stationSave = Save.Stations.FirstOrDefault(s => s.Id == definition.Id);
                if (stationSave == null)
                {
                    stationSave = new StationSaveData
                    {
                        Id = definition.Id,
                        Unlocked = definition.UnlockCost <= 0,
                        Level = 1,
                        Progress = 0
                    };
                    Save.Stations.Add(stationSave);
                }

                if (definition.UnlockCost <= 0) stationSave.Unlocked = true;
                if (stationSave.Level <= 0) stationSave.Level = 1;

                stations.Add(new StationRuntime(definition, stationSave));
            }

            if (stations.Count > 0 && (string.IsNullOrEmpty(SelectedStationId) || FindStation(SelectedStationId) == null))
            {
                StationRuntime firstUnlocked = stations.FirstOrDefault(s => s.Save.Unlocked);
                SelectedStationId = (firstUnlocked ?? stations[0]).Definition.Id;
            }
        }

        public void SaveGame()
        {
            if (Save == null) return;
            Save.LastSaveUnix = IdleGameBalance.UnixNow();
            PlayerPrefs.SetString(IdleGameBalance.SaveKey, UnityEngine.JsonUtility.ToJson(Save));
            PlayerPrefs.Save();
        }

        private bool Spend(double amount)
        {
            if (Save.Cash < amount) return false;
            Save.Cash -= amount;
            return true;
        }

        private void AddCash(double amount)
        {
            if (amount <= 0 || double.IsNaN(amount) || double.IsInfinity(amount)) return;
            Save.Cash += amount;
        }

        private StationRuntime FindStation(string stationId)
        {
            return stations.FirstOrDefault(s => s.Definition.Id == stationId);
        }

        private double CalculateIncomePerSecond(bool ignoreBoost = false)
        {
            double boost = ignoreBoost ? 1d : BoostMultiplier;
            double total = 0;
            foreach (StationRuntime station in stations)
            {
                total += station.IncomePerSecond(PrestigeMultiplier, boost);
            }
            return total;
        }

        private void Toast(string message)
        {
            OnToast?.Invoke(message);
        }

        private void RaiseStateChanged()
        {
            OnStateChanged?.Invoke();
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace IdleRepairTycoon
{
    [Serializable]
    public sealed class IdleGameSaveData
    {
        public double Cash = 0;
        public int ReputationStars = 0;
        public long LastSaveUnix = 0;
        public long BoostUntilUnix = 0;
        public List<StationSaveData> Stations = new List<StationSaveData>();
    }

    [Serializable]
    public sealed class StationSaveData
    {
        public string Id;
        public bool Unlocked;
        public int Level;
        public float Progress;
    }

    public sealed class StationDefinition
    {
        public string Id;
        public string Title;
        public string Description;
        public string Emoji;
        public double BaseProfit;
        public double UnlockCost;
        public float BaseDuration;
        public int RequiredReputation;

        public StationDefinition(string id, string title, string description, string emoji, double baseProfit, double unlockCost, float baseDuration, int requiredReputation = 0)
        {
            Id = id;
            Title = title;
            Description = description;
            Emoji = emoji;
            BaseProfit = baseProfit;
            UnlockCost = unlockCost;
            BaseDuration = baseDuration;
            RequiredReputation = requiredReputation;
        }
    }

    public sealed class StationRuntime
    {
        public StationDefinition Definition;
        public StationSaveData Save;

        public StationRuntime(StationDefinition definition, StationSaveData save)
        {
            Definition = definition;
            Save = save;
        }

        public int Level => Mathf.Max(1, Save.Level);

        public double ProfitPerJob(double prestigeMultiplier, double boostMultiplier)
        {
            double levelMultiplier = Math.Pow(1.18, Level - 1);
            return Definition.BaseProfit * levelMultiplier * prestigeMultiplier * boostMultiplier;
        }

        public float DurationSeconds()
        {
            float levelSpeedBonus = 1f + ((Level - 1) * 0.035f);
            return Mathf.Max(0.35f, Definition.BaseDuration / levelSpeedBonus);
        }

        public double IncomePerSecond(double prestigeMultiplier, double boostMultiplier)
        {
            if (!Save.Unlocked) return 0;
            return ProfitPerJob(prestigeMultiplier, boostMultiplier) / DurationSeconds();
        }

        public double UpgradeCost()
        {
            return Math.Ceiling(Definition.BaseProfit * 6.5d * Math.Pow(1.22d, Level - 1));
        }

        public float NormalizedProgress()
        {
            if (!Save.Unlocked) return 0;
            return Mathf.Clamp01(Save.Progress / DurationSeconds());
        }
    }

    public static class IdleGameBalance
    {
        public const string SaveKey = "IdleRepairTycoon_Save_v1";
        public const double StartingCash = 60;
        public const int MaxOfflineSeconds = 8 * 60 * 60;
        public const double OfflineIncomeRate = 0.55d;
        public const int RewardedBoostSeconds = 5 * 60;
        public const double RewardedBoostMultiplier = 2.0d;
        public const double PackageRewardSeconds = 90d;
        public const double PrestigeCost = 250000d;

        public static readonly List<StationDefinition> StationDefinitions = new List<StationDefinition>
        {
            new StationDefinition(
                "film",
                "Películas",
                "Aplicação rápida. Pouco lucro, muita velocidade.",
                "▣",
                baseProfit: 12,
                unlockCost: 0,
                baseDuration: 2.6f
            ),
            new StationDefinition(
                "battery",
                "Baterias",
                "Serviço popular com ticket médio melhor.",
                "▤",
                baseProfit: 65,
                unlockCost: 380,
                baseDuration: 5.2f
            ),
            new StationDefinition(
                "screen",
                "Telas",
                "Consertos mais caros e mais demorados.",
                "▥",
                baseProfit: 260,
                unlockCost: 3200,
                baseDuration: 9.5f
            ),
            new StationDefinition(
                "notebook",
                "Notebooks",
                "Diagnóstico avançado, ótimo lucro.",
                "▧",
                baseProfit: 1100,
                unlockCost: 35000,
                baseDuration: 16.5f
            ),
            new StationDefinition(
                "premium",
                "Laboratório Premium",
                "Reparos de placa e serviços especiais.",
                "▩",
                baseProfit: 6200,
                unlockCost: 180000,
                baseDuration: 28f,
                requiredReputation: 1
            )
        };

        public static string FormatMoney(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value)) value = 0;
            string sign = value < 0 ? "-" : "";
            value = Math.Abs(value);

            if (value < 1000) return sign + "R$ " + value.ToString("0");
            if (value < 1_000_000) return sign + "R$ " + (value / 1000d).ToString("0.##") + " mil";
            if (value < 1_000_000_000) return sign + "R$ " + (value / 1_000_000d).ToString("0.##") + " mi";
            if (value < 1_000_000_000_000) return sign + "R$ " + (value / 1_000_000_000d).ToString("0.##") + " bi";
            return sign + "R$ " + (value / 1_000_000_000_000d).ToString("0.##") + " tri";
        }

        public static long UnixNow()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
    }
}

using UnityEngine;

namespace IdleRepairTycoon
{
    public static class IdleGameBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateGame()
        {
            if (Object.FindAnyObjectByType<IdleGameController>() != null) return;

            GameObject root = new GameObject("Idle Repair Tycoon");
            Object.DontDestroyOnLoad(root);
            root.AddComponent<IdleGameController>();
        }
    }
}

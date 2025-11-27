using UnityEngine;

namespace Managers
{
    public class PauseManager : MonoBehaviour
    {
        // public static PauseManager Instance;

        public static bool IsGamePaused { get; private set; } = false;

        public static void SetPause(bool pause)
        {
            IsGamePaused = pause;
            AdjustTimescale();
        }

        private static void AdjustTimescale()
        {
            Time.timeScale = IsGamePaused ? 0f : 1f;
        }
        
        // private void Awake()
        // {
        //     if (Instance == null)
        //     {
        //         Instance = this;
        //         // DontDestroyOnLoad(gameObject);
        //     }
        //     else
        //     {
        //         Destroy(gameObject);
        //     }
        // }
    }
}
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace Managers
{
    public class PauseManager : MonoBehaviour
    {
        public static PauseManager Instance;
        [SerializeField] private GameObject pauseMenu;

        public static bool IsGamePaused { get; private set; } = false;

        public static void SetPause(bool pause)
        {
            IsGamePaused = pause;
        }

        public static void PauseGame(bool pause)
        {
            IsGamePaused = pause;
            Instance?.AdjustTimescale();
            Instance?.TogglePauseMenu();
        }

        private void AdjustTimescale()
        {
            Time.timeScale = IsGamePaused ? 0f : 1f;
        }

        private void TogglePauseMenu()
        {
            pauseMenu.SetActive(IsGamePaused);
        }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                // DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
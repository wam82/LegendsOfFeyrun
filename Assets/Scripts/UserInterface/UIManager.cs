using System;
using Character;
using Combat;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UserInterface
{
    public class UIManager :  MonoBehaviour
    {
        public static UIManager Instance;

        private PlayableCharacter _player;

        [SerializeField] private GameObject interactPanel;
        [SerializeField] private TMP_Text interactText;
        
        public void ShowInteractPrompt()
        {
            PlayerInput playerInput = _player.GetComponent<PlayerInput>();
            InputAction interactAction = playerInput.actions["Interact"];

            string bindingString = interactAction.GetBindingDisplayString();
            
            interactText.text = $"Press {bindingString} to interact";
            interactPanel.SetActive(true);
        }

        public void HideInteractPrompt()
        {
            interactPanel.SetActive(false);
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
        
        private void Start()
        {
            if (!_player)
            {
                if (CombatManager.Instance.player)
                {
                    _player = CombatManager.Instance.player.GetComponent<PlayableCharacter>();
                }
            }
        }
    }
}
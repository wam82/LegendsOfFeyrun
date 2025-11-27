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
        
        // public PlayerInput playerInput;

        private PlayableCharacter _player;

        [Header("Interact Tooltip")]
        [SerializeField] private GameObject interactPanel;
        [SerializeField] private TMP_Text interactText;
        
        [Header("Health Bar")]
        [SerializeField] private RectTransform healthBar;
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private float healthBarHeight;
        [SerializeField] private float healthBarWidth;
        
        private float _characterMaxHealth;
        private float _characterCurrentHealth;

        public void InstantiateHealthBar(float maxHealth)
        {
            _characterMaxHealth = maxHealth;
            SetHealth(_characterMaxHealth);
        }

        public void SetHealth(float health)
        {
            _characterCurrentHealth = health;
            healthText.text = health.ToString("0");
            float newWidth =  (_characterCurrentHealth / _characterMaxHealth) * healthBarWidth;
            
            healthBar.sizeDelta = new Vector2(newWidth, healthBarHeight);
        }
        
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
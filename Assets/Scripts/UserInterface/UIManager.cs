using System;
using Character;
using Combat;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UserInterface
{
    public class UIManager :  MonoBehaviour
    {
        public static UIManager Instance;

        private PlayableCharacter _player;

        [Header("Dialogue Panel")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TMP_Text dialogueText;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private Image dialoguePortrait;

        public GameObject DialoguePanel => dialoguePanel;
        public TMP_Text DialogueText => dialogueText;
        public TMP_Text NameText => nameText;
        public Image DialoguePortrait => dialoguePortrait;

        [Header("Interact Tooltip")]
        [SerializeField] private GameObject interactPanel;
        [SerializeField] private TMP_Text interactText;
        
        [Header("Health Bar")]
        [SerializeField] private RectTransform healthBarFill;
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private float healthBarHeight;
        [SerializeField] private float healthBarWidth;

        [Header("Other Panels")] 
        [SerializeField] private GameObject deathScreen;
        [SerializeField] private GameObject endScreen;
        
        
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
            
            healthBarFill.sizeDelta = new Vector2(newWidth, healthBarHeight);
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

        public void ToggleDeathScreen(bool toggle)
        {
            if (!endScreen.activeInHierarchy)
            {
                deathScreen.SetActive(toggle);
            }
        }

        public void ToggleEndScreen(bool toggle)
        {
            if (!deathScreen.activeInHierarchy)
            {
                endScreen.SetActive(toggle);
            }
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
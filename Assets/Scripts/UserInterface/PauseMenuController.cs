using System;
using Managers;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace UserInterface
{
    public class PauseMenuController : MonoBehaviour
    {
        // private PlayerInput _playerInput;

        public InputActionAsset inputActions;

        private UIDocument _uiDocument;

        private Button[] _menuButtons;
        private Button _resumeButton;
        private Button _returnButton;
        private Button _quitButton;

        private int _selectedIndex;

        private InputActionMap _actionMap;
        
        
        private void OnNavigate(InputAction.CallbackContext context)
        {
            Vector2 nav = context.ReadValue<Vector2>();

            if (nav.y > 0)
            {
                _selectedIndex = Mathf.Max(0, _selectedIndex - 1);
            }

            if (nav.y < 0)
            {
                _selectedIndex = Mathf.Min(_menuButtons.Length - 1, _selectedIndex + 1);
            }
            
            _menuButtons[_selectedIndex].Focus();
        }

        private void OnSubmit(InputAction.CallbackContext context)
        {
            switch (_selectedIndex)
            {
                case 0: OnResumeButtonPressed(); break;
                case 1: OnReturnButtonPressed(); break;
                case 2: OnQuitButtonPressed(); break;
                default: OnResumeButtonPressed(); break;
            }
        }

        private void OnCancel(InputAction.CallbackContext context)
        {
            OnResumeButtonPressed();
        }

        private void OnResumeButtonPressed()
        {
            ButtonActionsUnsubscribe();
            inputActions.FindActionMap("Game").Enable();
            ControllerActionsUnsubscribe();
            _actionMap.Disable();
            PauseManager.SetPause(!PauseManager.IsGamePaused);
        }

        private void OnReturnButtonPressed()
        {
            Debug.Log("Return to main menu");
        }

        private void OnQuitButtonPressed()
        {
            Application.Quit();

#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
        }

        private void ButtonActionsSubscribe()
        {
            _resumeButton.clicked += OnResumeButtonPressed;
            _returnButton.clicked += OnReturnButtonPressed;
            _quitButton.clicked += OnQuitButtonPressed;
        }

        private void ButtonActionsUnsubscribe()
        {
            _resumeButton.clicked -= OnResumeButtonPressed;
            _returnButton.clicked -= OnReturnButtonPressed;
            _quitButton.clicked -= OnQuitButtonPressed;
        }

        private void ControllerActionsSubscribe()
        {
            InputAction navigate = _actionMap.FindAction("Navigate");
            navigate.performed += OnNavigate;
            
            InputAction submit = _actionMap.FindAction("Submit");
            submit.performed += OnSubmit;
            
            InputAction cancel = _actionMap.FindAction("Cancel");
            cancel.performed += OnCancel;
            
            InputAction pause = _actionMap.FindAction("Pause");
            pause.performed += OnCancel;
        }

        private void ControllerActionsUnsubscribe()
        {
            InputAction navigate = _actionMap.FindAction("Navigate");
            navigate.performed -= OnNavigate;
            
            InputAction submit = _actionMap.FindAction("Submit");
            submit.performed -= OnSubmit;
            
            InputAction cancel = _actionMap.FindAction("Cancel");
            cancel.performed -= OnCancel;
            
            InputAction pause = _actionMap.FindAction("Pause");
            pause.performed -= OnCancel;
        }

        private void OnEnable()
        {
            Debug.Log("OnEnable");
            
            VisualElement root = GetComponent<UIDocument>().rootVisualElement;
            
            _resumeButton = root.Q<Button>("ResumeButton");
            _returnButton = root.Q<Button>("ReturnButton");
            _quitButton = root.Q<Button>("QuitButton");
            
            _menuButtons = new[] { _resumeButton, _returnButton, _quitButton };
            
            _actionMap = inputActions.FindActionMap("UserInterface");
            ButtonActionsSubscribe();
            _actionMap.Enable();
            inputActions.FindActionMap("Game").Disable();
            ControllerActionsSubscribe();
            _menuButtons[0].Focus();
        }

        private void OnDisable()
        {
            ButtonActionsUnsubscribe();
            inputActions.FindActionMap("Game").Enable();
            ControllerActionsUnsubscribe();
            _actionMap.Disable();
        }
        
    }
}
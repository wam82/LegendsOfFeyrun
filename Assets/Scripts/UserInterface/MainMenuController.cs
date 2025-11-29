using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace UserInterface
{
    public class MainMenuController : MonoBehaviour
    {
        public InputActionAsset inputActions;
        
        private UIDocument _uiDocument;
        
        private Button[] _menuButtons;
        private Button _playButton;
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
                case 0: OnPlayButtonPressed(); break;
                case 1: OnQuitButtonPressed(); break;
                default: Debug.LogError("Something wrong happened in the main menu"); break;
            }
        }

        private void OnPlayButtonPressed()
        {
            ButtonActionsUnsubscribe();
            inputActions.FindActionMap("Game").Enable();
            ControllerActionsUnsubscribe();
            _actionMap.Disable();
            SceneManager.LoadScene("PrototypeLevel");
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
            _playButton.clicked += OnPlayButtonPressed;
            _quitButton.clicked += OnQuitButtonPressed;
        }

        private void ButtonActionsUnsubscribe()
        {
            _playButton.clicked += OnPlayButtonPressed;
            _quitButton.clicked -= OnQuitButtonPressed;
        }
        
        private void ControllerActionsSubscribe()
        {
            InputAction navigate = _actionMap.FindAction("Navigate");
            navigate.performed += OnNavigate;
            
            InputAction submit = _actionMap.FindAction("Submit");
            submit.performed += OnSubmit;
        }

        private void ControllerActionsUnsubscribe()
        {
            InputAction navigate = _actionMap.FindAction("Navigate");
            navigate.performed -= OnNavigate;
            
            InputAction submit = _actionMap.FindAction("Submit");
            submit.performed -= OnSubmit;
        }
        
        private void OnEnable()
        {
            VisualElement root = GetComponent<UIDocument>().rootVisualElement;
            
            _playButton = root.Q<Button>("PlayButton");
            _quitButton = root.Q<Button>("QuitButton");
            
            _menuButtons = new[] { _playButton, _quitButton };
            
            _actionMap = inputActions.FindActionMap("UserInterface");
            ButtonActionsSubscribe();
            _actionMap.Enable();
            inputActions.FindActionMap("Game").Disable();
            ControllerActionsSubscribe();
            _menuButtons[0].Focus();
        }
        
        // private void OnDisable()
        // {
        //     ButtonActionsUnsubscribe();
        //     inputActions.FindActionMap("Game").Enable();
        //     ControllerActionsUnsubscribe();
        //     _actionMap.Disable();
        // }
    }
}
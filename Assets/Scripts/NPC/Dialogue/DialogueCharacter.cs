using System.Collections;
using Character;
using Environment.Interfaces;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UserInterface;

namespace NPC.Dialogue
{
    public class DialogueCharacter : MonoBehaviour, IInteractable
    {
        [SerializeField] private DialogueData dialogueData;
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TMP_Text dialogueText;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private Image dialoguePortrait;
        
        private int _dialogueIndex;
        
        private bool _isTyping;
        private bool _isDialogueActive;
        private bool _isPlayerInTrigger;

        public bool CanInteract()
        {
            return !_isDialogueActive && _isPlayerInTrigger;
        }

        public void Interact()
        {
            if (!dialogueData)
            {
                return;
            }

            if (_isDialogueActive)
            {
                NextLine();
            }
            else
            {
                StartDialogue();
            }
        }

        private void StartDialogue()
        {
            _isDialogueActive = true;
            _dialogueIndex = 0;
            
            nameText.SetText(dialogueData.characterName);
            dialoguePortrait.sprite = dialogueData.portrait;
            dialoguePanel.SetActive(true);
            PauseManager.SetPause(true);
            
            StartCoroutine(TypeLine());
        }

        private void NextLine()
        {
            if (_isTyping)
            {
                StopAllCoroutines();
                dialogueText.SetText(dialogueData.dialogueLines[_dialogueIndex]);
                _isTyping = false;
            }
            else if (++_dialogueIndex < dialogueData.dialogueLines.Length)
            {
                StartCoroutine(TypeLine());
            }
            else
            {
                EndDialogue();
            }
        }

        private IEnumerator TypeLine()
        {
            _isTyping = true;
            dialogueText.SetText("");

            foreach (char letter in dialogueData.dialogueLines[_dialogueIndex])
            {
                dialogueText.text += letter;
                yield return new WaitForSeconds(dialogueData.typingSpeed);
            }
            
            _isTyping = false;

            if (dialogueData.autoProgressLines.Length > _dialogueIndex && dialogueData.autoProgressLines[_dialogueIndex])
            {
                yield return new WaitForSeconds(dialogueData.autoProgressDelay);
                NextLine();
            }
        }

        private void EndDialogue()
        {
            StopAllCoroutines();
            _isDialogueActive = false;
            dialogueText.SetText("");
            dialoguePanel.SetActive(false);
            PauseManager.SetPause(false);
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                other.gameObject.GetComponent<PlayableCharacter>().interactableObject =
                    gameObject.GetComponent<IInteractable>();
                _isPlayerInTrigger = true;
                
                UIManager.Instance.ShowInteractPrompt();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                other.gameObject.GetComponent<PlayableCharacter>().interactableObject = null;
                _isPlayerInTrigger = false;
                
                UIManager.Instance.HideInteractPrompt();
            }
        }
    }
}
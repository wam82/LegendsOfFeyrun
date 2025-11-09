using System.Collections;
using Character;
using Environment.Interfaces;
using UnityEngine;

namespace NPC.Dialogue
{
    public class DialogueCharacter : MonoBehaviour, IInteractable
    {
        [SerializeField] private DialogueData dialogueData;
        
        public DialogueData DialogueData => dialogueData;
        
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

        private void Speak(string sentence)
        {
            Debug.Log(dialogueData.characterName + ": " + sentence);
        }

        private void StartDialogue()
        {
            _isDialogueActive = true;
            _dialogueIndex = 0;
            
            // StartCoroutine(TypeLine());
            StartCoroutine(TestSpeak());
        }

        private void NextLine()
        {
            if (_isTyping)
            {
                StopAllCoroutines();
                //ui.setText(dialogueData.dialogueLines[_dialogueIndex]);
                _isTyping = false;
            }
            else if (++_dialogueIndex < dialogueData.dialogueLines.Length)
            {
                StartCoroutine(TestSpeak());
            }
            else
            {
                EndDialogue();
            }
        }

        private IEnumerator TestSpeak()
        {
            Speak(dialogueData.dialogueLines[_dialogueIndex]);
            
            if (dialogueData.autoProgressLines.Length > _dialogueIndex && dialogueData.autoProgressLines[_dialogueIndex])
            {
                yield return new WaitForSeconds(dialogueData.autoProgressDelay);
                NextLine();
            }
        }

        private IEnumerator TypeLine()
        {
            _isTyping = true;

            foreach (char letter in dialogueData.dialogueLines[_dialogueIndex])
            {
                //ui.text += letter;
                yield return new WaitForSeconds(dialogueData.typingSpeed);
            }
            
            _isTyping = false;

            if (dialogueData.autoProgressLines.Length > _dialogueIndex && dialogueData.autoProgressLines[_dialogueIndex])
            {
                yield return new WaitForSeconds(dialogueData.autoProgressDelay);
                // NextLine();
            }
        }

        public void EndDialogue()
        {
            StopAllCoroutines();
            _isDialogueActive = false;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                other.gameObject.GetComponent<PlayableCharacter>().interactableObject =
                    gameObject.GetComponent<IInteractable>();
                _isPlayerInTrigger = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                other.gameObject.GetComponent<PlayableCharacter>().interactableObject = null;
                _isPlayerInTrigger = false;
            }
        }
    }
}
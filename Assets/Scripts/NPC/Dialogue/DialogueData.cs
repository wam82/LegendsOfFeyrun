using UnityEngine;

namespace NPC.Dialogue
{
    [CreateAssetMenu(menuName = "NPC Dialogue", fileName = "NewDialogue")]
    public class DialogueData : ScriptableObject
    {
        public string characterName;
        public Sprite portrait;
        public string[] dialogueLines;
        public bool[] autoProgressLines;
        public float autoProgressDelay = 1.5f;
        public float typingSpeed = 0.05f;
        public AudioClip[] dialogueClips;
        public float voicePitch = 1f;
    }
}

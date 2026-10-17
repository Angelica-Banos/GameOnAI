using UnityEngine;
using System.Collections.Generic;

public class DialogueTrigger : MonoBehaviour {
    [Header("Líneas de Diálogo")]
    public List<DialogueLine> dialogueLines;

    [Tooltip("Llama a esta función desde eventos de Unity (como interact_with_npc) para iniciar el diálogo.")]
    public void TriggerDialogue() {
        if (DialogueManager.Instance != null) {
            DialogueManager.Instance.StartDialogue(dialogueLines);
        } else {
            Debug.LogError("No se encontró un DialogueManager en la escena.");
        }
    }
}

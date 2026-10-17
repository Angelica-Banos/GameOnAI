using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro para textos con mejor calidad
using System.Collections.Generic;

[System.Serializable]
public class DialogueLine {
    [Header("Configuración del Personaje")]
    public string characterName;
    [Tooltip("La imagen del personaje que está hablando.")]
    public Sprite characterSprite;
    [Tooltip("¿El personaje aparece a la izquierda? (Falso para derecha)")]
    public bool isLeftCharacter = true;
    
    [Header("Texto")]
    [TextArea(3, 5)]
    public string sentence;
}

public class DialogueManager : MonoBehaviour {
    public static DialogueManager Instance;
    
    // Bandera estática para pausar/congelar a los jugadores y enemigos
    public static bool IsDialogueActive = false;

    [Header("Referencias de UI (Asignar en Inspector)")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public Image leftCharacterImage;
    public Image rightCharacterImage;

    private Queue<DialogueLine> sentences = new Queue<DialogueLine>();

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    public void StartDialogue(List<DialogueLine> lines) {
        IsDialogueActive = true;
        dialoguePanel.SetActive(true);
        sentences.Clear();

        foreach(var line in lines) {
            sentences.Enqueue(line);
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence() {
        if (sentences.Count == 0) {
            EndDialogue();
            return;
        }

        DialogueLine currentLine = sentences.Dequeue();
        
        if(nameText != null) nameText.text = currentLine.characterName;
        if(dialogueText != null) dialogueText.text = currentLine.sentence;

        // Configuración de los sprites de los personajes a los lados
        if (currentLine.isLeftCharacter) {
            if(leftCharacterImage != null) {
                leftCharacterImage.sprite = currentLine.characterSprite;
                leftCharacterImage.gameObject.SetActive(true);
            }
            if(rightCharacterImage != null) rightCharacterImage.gameObject.SetActive(false);
        } else {
            if(rightCharacterImage != null) {
                rightCharacterImage.sprite = currentLine.characterSprite;
                rightCharacterImage.gameObject.SetActive(true);
            }
            if(leftCharacterImage != null) leftCharacterImage.gameObject.SetActive(false);
        }
    }

    private void Update() {
        if (IsDialogueActive && Input.GetKeyDown(KeyCode.Space)) {
            DisplayNextSentence();
        }
    }

    public void EndDialogue() {
        IsDialogueActive = false;
        if(dialoguePanel != null) dialoguePanel.SetActive(false);
    }
}

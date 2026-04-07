using UnityEngine;
using TMPro;

/// <summary>
/// Generic reusable NPC for the Village Hub.
/// 
/// Each NPC has a list of dialogue lines. When the player enters the trigger zone
/// and presses Interact, it cycles through the lines one by one.
/// After the last line, the dialogue closes and resets to the beginning.
/// 
/// Scene Setup:
/// - Add a Collider2D (Is Trigger) to the NPC GameObject.
/// - Fill in the dialogueLines array in the Inspector for each NPC.
/// - Assign the dialogueBox UI GameObject and dialogueText TMP component.
/// - All NPCs share the same dialogue UI panel (assign the same one to all).
/// </summary>
public class HubNPC : MonoBehaviour, IHubInteractable
{
    [Header("NPC Settings")]
    [SerializeField] private string npcName;

    [TextArea(2, 5)]
    [SerializeField] private string[] dialogueLines;

    [Header("UI References")]
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private GameObject interactPrompt; // "Press [Interact] to talk"

    private int currentLine = 0;
    private bool isOpen = false;

    private void Start()
    {
        CloseDialogue();
    }

    /// <summary>
    /// Called by HubPlayerController when the player presses Interact inside the zone.
    /// Advances dialogue line by line, closes after the last one.
    /// </summary>
    public void Interact()
    {
        if (!isOpen)
        {
            OpenDialogue();
            return;
        }

        currentLine++;

        if (currentLine >= dialogueLines.Length)
        {
            CloseDialogue();
            currentLine = 0;
        }
        else
        {
            ShowLine(currentLine);
        }
    }

    private void OpenDialogue()
    {
        isOpen = true;
        currentLine = 0;

        if (dialogueBox != null) dialogueBox.SetActive(true);
        if (interactPrompt != null) interactPrompt.SetActive(false);

        ShowLine(currentLine);
    }

    private void CloseDialogue()
    {
        isOpen = false;
        if (dialogueBox != null) dialogueBox.SetActive(false);
        if (interactPrompt != null) interactPrompt.SetActive(false);
    }

    private void ShowLine(int index)
    {
        if (nameText != null) nameText.text = npcName;
        if (dialogueText != null) dialogueText.text = dialogueLines[index];
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        HubPlayerController player = other.GetComponent<HubPlayerController>();
        if (player != null) player.SetInteractable(this);

        if (interactPrompt != null) interactPrompt.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        HubPlayerController player = other.GetComponent<HubPlayerController>();
        if (player != null) player.ClearInteractable(this);

        CloseDialogue();
    }
}
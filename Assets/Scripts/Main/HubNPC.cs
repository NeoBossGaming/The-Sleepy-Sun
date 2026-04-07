using UnityEngine;
using TMPro;

/// <summary>
/// Generic reusable NPC for the Village Hub.
/// Supports two-way dialogue — each line is tagged with a Speaker (NPC or Richelle).
/// The correct nameplate and dialogue box activates depending on who is speaking.
///
/// Scene Setup:
/// - Two separate dialogue box UI panels: one for the NPC (bottom-left), one for Richelle (bottom-right).
/// - Each panel has its own nameText and dialogueText TMP components.
/// - Fill in the dialogueLines array in the Inspector. Set Speaker per line.
/// - All NPCs can share the same Canvas UI — just wire the same references.
/// </summary>
public class HubNPC : MonoBehaviour, IHubInteractable
{
    // ── Data ─────────────────────────────────────────────────────────────────

    public enum Speaker { NPC, Richelle }

    [System.Serializable]
    public class DialogueLine
    {
        public Speaker speaker;

        [TextArea(2, 4)]
        public string text;
    }

    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("NPC Settings")]
    [SerializeField] private string npcName = "NPC";

    [SerializeField] private DialogueLine[] dialogueLines;

    [Header("NPC Dialogue Box  (e.g. bottom-left)")]
    [SerializeField] private GameObject npcDialogueBox;
    [SerializeField] private TextMeshProUGUI npcNameText;
    [SerializeField] private TextMeshProUGUI npcDialogueText;

    [Header("Richelle Dialogue Box  (e.g. bottom-right)")]
    [SerializeField] private GameObject richelleDialogueBox;
    [SerializeField] private TextMeshProUGUI richelleNameText;   // Will always say "Richelle"
    [SerializeField] private TextMeshProUGUI richelleDialogueText;

    [Header("Interact Prompt")]
    [SerializeField] private GameObject interactPrompt;          // "Press [Interact] to talk"

    // ── State ─────────────────────────────────────────────────────────────────

    private int currentLine = 0;
    private bool isOpen = false;

    // ── Unity ─────────────────────────────────────────────────────────────────

    private void Start()
    {
        CloseDialogue();
    }

    // ── IHubInteractable ──────────────────────────────────────────────────────

    /// <summary>
    /// Opens dialogue on first press. Advances line by line. Closes after the last line.
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
        }
        else
        {
            ShowLine(currentLine);
        }
    }

    // ── Dialogue Flow ─────────────────────────────────────────────────────────

    private void OpenDialogue()
    {
        isOpen = true;
        currentLine = 0;

        if (interactPrompt != null) interactPrompt.SetActive(false);

        ShowLine(currentLine);
    }

    private void CloseDialogue()
    {
        isOpen = false;
        currentLine = 0;

        SetBoxActive(Speaker.NPC, false);
        SetBoxActive(Speaker.Richelle, false);

        if (interactPrompt != null) interactPrompt.SetActive(false);
    }

    private void ShowLine(int index)
    {
        DialogueLine line = dialogueLines[index];

        // Hide both boxes first, then activate only the correct one
        SetBoxActive(Speaker.NPC, false);
        SetBoxActive(Speaker.Richelle, false);
        SetBoxActive(line.speaker, true);

        if (line.speaker == Speaker.NPC)
        {
            if (npcNameText != null)     npcNameText.text     = npcName;
            if (npcDialogueText != null) npcDialogueText.text = line.text;
        }
        else
        {
            if (richelleNameText != null)     richelleNameText.text     = "Richelle";
            if (richelleDialogueText != null) richelleDialogueText.text = line.text;
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void SetBoxActive(Speaker speaker, bool active)
    {
        if (speaker == Speaker.NPC)
        {
            if (npcDialogueBox != null) npcDialogueBox.SetActive(active);
        }
        else
        {
            if (richelleDialogueBox != null) richelleDialogueBox.SetActive(active);
        }
    }

    // ── Trigger ───────────────────────────────────────────────────────────────

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
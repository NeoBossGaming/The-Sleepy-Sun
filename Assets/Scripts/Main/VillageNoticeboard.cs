using UnityEngine;
using TMPro;

/// <summary>
/// An interactive noticeboard in the Village Hub.
///
/// Displays a contextual hint about which Wake-Up Brew ingredients are still needed.
/// Mirrors the HubNPC interaction pattern (implements IHubInteractable) so the same
/// player controller wires it up automatically.
///
/// Scene Setup:
///   - Add to a GameObject with a trigger Collider2D.
///   - Wire the same npcDialogueBox / npcNameText / npcDialogueText TMP references
///     used by other hub NPCs.
///   - No dialogueLines array needed — messages are generated from GameManager state.
/// </summary>
public class VillageNoticeboard : MonoBehaviour, IHubInteractable
{
    // ── Inspector ──────────────────────────────────────────────────────────────

    [Header("Noticeboard Dialogue Box")]
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI bodyText;

    [Header("Interact Prompt")]
    [SerializeField] private GameObject interactPrompt;

    [Header("Board Label")]
    [SerializeField] private string boardName = "Village Noticeboard";

    // ── State ──────────────────────────────────────────────────────────────────

    private bool isOpen = false;
    private int currentLine = 0;
    private string[] lines;

    // ── Unity ──────────────────────────────────────────────────────────────────

    private void Start()
    {
        CloseBoard();
    }

    // ── IHubInteractable ───────────────────────────────────────────────────────

    public void Interact()
    {
        if (!isOpen)
        {
            OpenBoard();
            return;
        }

        currentLine++;

        if (currentLine >= lines.Length)
            CloseBoard();
        else
            ShowLine(currentLine);
    }

    // ── Board Flow ─────────────────────────────────────────────────────────────

    private void OpenBoard()
    {
        lines = BuildLines();
        currentLine = 0;
        isOpen = true;

        if (interactPrompt != null) interactPrompt.SetActive(false);
        if (dialogueBox    != null) dialogueBox.SetActive(true);
        if (nameText       != null) nameText.text = boardName;

        ShowLine(0);
    }

    private void CloseBoard()
    {
        isOpen      = false;
        currentLine = 0;

        if (dialogueBox    != null) dialogueBox.SetActive(false);
        if (interactPrompt != null) interactPrompt.SetActive(false);
    }

    private void ShowLine(int index)
    {
        if (bodyText != null) bodyText.text = lines[index];
    }

    // ── Message Generation ─────────────────────────────────────────────────────

    private string[] BuildLines()
    {
        GameManager gm = FindFirstObjectByType<GameManager>();

        if (gm == null)
        {
            return new string[]
            {
                "\" The sun slumbers... someone must brew\n   the Wake-Up Brew to wake it! \""
            };
        }

        bool leaf   = gm.HasSilverLeaf;
        bool nectar = gm.HasCaveNectar;
        bool fish   = gm.HasStardustFish;
        bool ginger = gm.HasGoldenGinger;

        int count = (leaf ? 1 : 0) + (nectar ? 1 : 0) + (fish ? 1 : 0) + (ginger ? 1 : 0);

        if (count == 0)
        {
            return new string[]
            {
                "VILLAGE NOTICEBOARD\n\n" +
                "The Sleepy Sun needs the Wake-Up Brew!\n" +
                "Four ingredients must be gathered:\n\n" +
                "  ☐  Silver Leaf  — Wind-Chiming Forest\n" +
                "  ☐  Cave Nectar  — Crystal Caves\n" +
                "  ☐  Stardust Fish — Starfish Pond\n" +
                "  ☐  Golden Ginger — Tall Grass",

                "Speak to the villagers — they know the\n" +
                "paths well. Good luck, Richelle!"
            };
        }

        if (gm.checkWinCondition())
        {
            return new string[]
            {
                "VILLAGE NOTICEBOARD\n\n" +
                "ALL INGREDIENTS GATHERED!\n\n" +
                "  ✓  Silver Leaf\n" +
                "  ✓  Cave Nectar\n" +
                "  ✓  Stardust Fish\n" +
                "  ✓  Golden Ginger",

                "The Grand Teapot awaits.\n" +
                "Brew the Wake-Up Brew and wake the sun!"
            };
        }

        // Partial progress
        string checklist =
            $"  {(leaf   ? "✓" : "☐")}  Silver Leaf\n" +
            $"  {(nectar ? "✓" : "☐")}  Cave Nectar\n" +
            $"  {(fish   ? "✓" : "☐")}  Stardust Fish\n" +
            $"  {(ginger ? "✓" : "☐")}  Golden Ginger";

        return new string[]
        {
            $"VILLAGE NOTICEBOARD\n\n" +
            $"Ingredients gathered: {count} / 4\n\n" +
            checklist,

            count >= 3
                ? "Almost there! One more ingredient\nand the brew can be made."
                : "Keep exploring the paths — \nthe village is counting on you!"
        };
    }

    // ── Trigger ────────────────────────────────────────────────────────────────

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

        CloseBoard();
    }
}

using UnityEngine;
using System.Collections;

/// <summary>
/// Individual lily-pad leaf in the Wind-Chiming Forest trial.
///
/// Each leaf scrolls upward at scrollSpeed (set by the GameManager on spawn).
/// The GameManager calls TriggerShake() on random leaves each beat cycle.
/// If the player is on this leaf when it collapses, it reports death to the GameManager.
///
/// Inspector Setup:
///   - Assign this to a leaf sprite GameObject.
///   - Collider2D on the same object: Is Trigger = true.
///   - Tag this GameObject "Leaf".
///   - Do NOT set scrollSpeed or laneIndex in Inspector — GameManager sets those at runtime.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class WindChimingForestLeaf : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField] private float shakeDuration     = 0.8f;  // Duration of shake before collapse
    [SerializeField] private float shakeIntensity    = 0.08f; // Max x-axis wobble (world units)
    [SerializeField] private float collapsedDuration = 1.2f;  // Time the leaf stays gone

    // Set by WindChimingForestGameManager on spawn — don't touch in Inspector
    [HideInInspector] public float scrollSpeed;
    [HideInInspector] public int laneIndex;

    public bool isSafe = true;

    private SpriteRenderer sr;
    private Collider2D col;
    private bool isShaking;
    private bool playerIsOn;
    private Coroutine shakeCoroutine;
    private WindChimingForestGameManager gameManager;

    void Awake()
    {
        sr  = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        col.isTrigger = true;
        gameManager = FindObjectOfType<WindChimingForestGameManager>();
    }

    void Update()
    {
        // Scroll upward — same speed as the player, so relative positions stay constant
        transform.Translate(Vector3.up * scrollSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Begins the shake → collapse → respawn sequence.
    /// Called by WindChimingForestGameManager on each beat cycle.
    /// Ignored if already shaking or already collapsed.
    /// </summary>
    public void TriggerShake()
    {
        if (isShaking || !isSafe) return;
        shakeCoroutine = StartCoroutine(ShakeSequence());
    }

    /// <summary>
    /// Hard-resets this leaf to full safe state.
    /// Called by the GameManager when the player respawns, so collapsing leaves
    /// don't carry over and softlock the player.
    /// </summary>
    public void ResetLeaf()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }

        isShaking   = false;
        playerIsOn  = false;
        isSafe      = true;
        sr.enabled  = true;
        col.enabled = true;
        transform.localPosition = Vector3.zero; // Clear any lingering shake offset
    }

    private IEnumerator ShakeSequence()
    {
        isShaking = true;
        Vector3 originalLocalPos = transform.localPosition;
        float elapsed = 0f;

        // 1 — Shake (warning phase)
        while (elapsed < shakeDuration)
        {
            float xOffset = Random.Range(-shakeIntensity, shakeIntensity);
            transform.localPosition = originalLocalPos + new Vector3(xOffset, 0f, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 2 — Collapse
        transform.localPosition = originalLocalPos;
        isSafe      = false;
        sr.enabled  = false;
        col.enabled = false;

        if (playerIsOn) gameManager?.PlayerFell();

        // 3 — Stay collapsed
        yield return new WaitForSeconds(collapsedDuration);

        // 4 — Respawn
        isSafe      = true;
        sr.enabled  = true;
        col.enabled = true;
        isShaking   = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerIsOn = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerIsOn = false;
    }
}
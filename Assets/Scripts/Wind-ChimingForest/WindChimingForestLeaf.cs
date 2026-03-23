using UnityEngine;
using System.Collections;

/// <summary>
/// Individual lily-pad leaf in the Wind-Chiming Forest trial.
/// Manages its own shake → collapse → respawn cycle.
///
/// Inspector Setup:
///   - Attach to each leaf sprite GameObject
///   - Requires a SpriteRenderer and a Collider2D (NOT a trigger — player physically stands on it)
///   - Tag this GameObject as "Leaf"
///   - Adjust shakeDuration, shakeIntensity, collapsedDuration in Inspector per leaf
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class WindChimingForestLeaf : MonoBehaviour
{
    [Header("State")]
    public bool isSafe = true;

    [Header("Timing")]
    [SerializeField] private float shakeDuration    = 1f;   // How long it shakes before collapsing
    [SerializeField] private float shakeIntensity   = 0.1f; // Max x-axis shake offset
    [SerializeField] private float collapsedDuration = 1f;  // How long it stays gone before respawning

    private Vector3 originalLocalPos;
    private SpriteRenderer spriteRenderer;
    private Collider2D leafCollider;
    private bool isShaking = false;

    void Start()
    {
        originalLocalPos = transform.localPosition;
        spriteRenderer   = GetComponent<SpriteRenderer>();
        leafCollider     = GetComponent<Collider2D>();
    }

    /// <summary>
    /// Starts the shake → collapse → respawn sequence.
    /// Safe to call any time — ignored if already shaking or already collapsed.
    /// </summary>
    public void TriggerShake()
    {
        if (!isShaking && isSafe)
            StartCoroutine(ShakeSequence());
    }

    private IEnumerator ShakeSequence()
    {
        isShaking = true;

        // 1. Shake
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            float xOffset = Random.Range(-shakeIntensity, shakeIntensity);
            transform.localPosition = originalLocalPos + new Vector3(xOffset, 0f, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 2. Collapse
        transform.localPosition = originalLocalPos;
        isSafe = false;
        spriteRenderer.enabled = false;
        leafCollider.enabled   = false;

        // 3. Wait
        yield return new WaitForSeconds(collapsedDuration);

        // 4. Respawn
        spriteRenderer.enabled = true;
        leafCollider.enabled   = true;
        isSafe    = true;
        isShaking = false;
    }
}
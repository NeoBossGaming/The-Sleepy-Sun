using UnityEngine;

/// <summary>
/// TallGrass player controller. Extends the base PlayerMovement.
///
/// EXTENSIONS:
///   - Adds IsHidden: set true when inside a "TallGrass" trigger, false when outside.
///   - Reduces sprite opacity to 50% when hidden as a visual cue.
///   - Movement itself (4-directional top-down) is fully inherited from PlayerMovement.
///
/// DESIGN: This script does not handle capture logic.
/// It only manages its own stealth state and notifies nothing.
/// The TallGrassShadowBird reads IsHidden and notifies the Manager.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class TallGrassPlayerController : PlayerMovement
{
    // --- State ---
    private bool isHidden = false;

    // --- Cached ---
    private SpriteRenderer spriteRenderer;

    // Opacity values for visual feedback
    private const float HiddenAlpha = 0.45f;
    private const float VisibleAlpha = 1.0f;

    // --- Public API ---
    public bool IsHidden => isHidden;

    // -------------------------------------------------------------------------
    // Lifecycle
    // -------------------------------------------------------------------------

    protected override void Start()
    {
        base.Start();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // -------------------------------------------------------------------------
    // Stealth Trigger Detection
    // -------------------------------------------------------------------------

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("TallGrass"))
        {
            isHidden = true;
            SetSpriteAlpha(HiddenAlpha);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("TallGrass"))
        {
            isHidden = false;
            SetSpriteAlpha(VisibleAlpha);
        }
    }

    // -------------------------------------------------------------------------
    // Private Helpers
    // -------------------------------------------------------------------------

    private void SetSpriteAlpha(float alpha)
    {
        if (spriteRenderer == null) return;
        Color c = spriteRenderer.color;
        c.a = alpha;
        spriteRenderer.color = c;
    }
}
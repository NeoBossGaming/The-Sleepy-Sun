using UnityEngine;

/// <summary>
/// Player controller for the Tall Grass Path trial.
/// Extends PlayerMovement with an isHidden state that activates when
/// the player is inside a "Grass"-tagged trigger collider.
///
/// Inspector Setup:
///   - Attach to player root GameObject alongside PlayerInput, Rigidbody2D, SpriteRenderer
///   - Rigidbody2D: Gravity Scale = 0, Freeze Rotation Z, Body Type = Dynamic
///   - Tag player as "Player"
///   - Grass patches: Collider2D with Is Trigger = true, tagged "Grass"
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class TallGrassPlayerController : PlayerMovement
{
    [Header("Hiding")]
    [SerializeField] private float hiddenAlpha = 0.5f;

    public bool isHidden = false;

    private SpriteRenderer spriteRenderer;
    private int grassOverlapCount = 0; // Tracks overlapping grass patches to avoid early un-hiding

    protected override void Start()
    {
        base.Start();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Grass")) return;

        grassOverlapCount++;
        isHidden = true;
        SetAlpha(hiddenAlpha);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Grass")) return;

        grassOverlapCount = Mathf.Max(0, grassOverlapCount - 1);

        // Only un-hide when fully out of all grass patches
        if (grassOverlapCount == 0)
        {
            isHidden = false;
            SetAlpha(1f);
        }
    }

    private void SetAlpha(float alpha)
    {
        Color c = spriteRenderer.color;
        spriteRenderer.color = new Color(c.r, c.g, c.b, alpha);
    }
}
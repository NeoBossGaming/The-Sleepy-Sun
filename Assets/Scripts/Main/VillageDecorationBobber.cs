using UnityEngine;

/// <summary>
/// Gentle sine-wave idle animation for decorative village objects.
///
/// Attach to any decoration (lantern, sign, flower, flag, etc.) to make it
/// softly bob up and down and/or rotate slightly, giving the hub a living feel.
///
/// Scene Setup:
///   - Attach to the decoration's root Transform.
///   - Mix and match bob + sway to taste via the Inspector.
///   - Use a random phase offset so multiple decorations don't move in sync.
/// </summary>
public class VillageDecorationBobber : MonoBehaviour
{
    // ── Bob (vertical) ────────────────────────────────────────────────────────

    [Header("Bob (vertical float)")]
    [SerializeField] private bool bobEnabled     = true;
    [SerializeField] private float bobAmplitude  = 0.06f;   // world units
    [SerializeField] private float bobSpeed      = 1.4f;    // cycles per second

    // ── Sway (Z rotation) ─────────────────────────────────────────────────────

    [Header("Sway (gentle tilt)")]
    [SerializeField] private bool swayEnabled    = false;
    [SerializeField] private float swayAmplitude = 3f;      // degrees
    [SerializeField] private float swaySpeed     = 0.8f;

    // ── Scale Pulse ───────────────────────────────────────────────────────────

    [Header("Scale Pulse (breathing)")]
    [SerializeField] private bool pulseEnabled   = false;
    [SerializeField] private float pulseAmount   = 0.04f;   // fraction of base scale
    [SerializeField] private float pulseSpeed    = 1.0f;

    // ── Phase offset so grouped decorations look organic ──────────────────────

    [Header("Phase")]
    [Tooltip("Leave at 0 to auto-randomise on Awake.")]
    [SerializeField] private float phaseOffset = 0f;

    // ── Cache ─────────────────────────────────────────────────────────────────

    private Vector3 originPosition;
    private Vector3 originScale;

    // ── Unity ─────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (phaseOffset == 0f)
            phaseOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Start()
    {
        originPosition = transform.localPosition;
        originScale    = transform.localScale;
    }

    private void Update()
    {
        float t = Time.time + phaseOffset;

        if (bobEnabled)
        {
            Vector3 pos = originPosition;
            pos.y += Mathf.Sin(t * bobSpeed * Mathf.PI * 2f) * bobAmplitude;
            transform.localPosition = pos;
        }

        if (swayEnabled)
        {
            float angle = Mathf.Sin(t * swaySpeed * Mathf.PI * 2f) * swayAmplitude;
            transform.localRotation = Quaternion.Euler(0f, 0f, angle);
        }

        if (pulseEnabled)
        {
            float scale = 1f + Mathf.Sin(t * pulseSpeed * Mathf.PI * 2f) * pulseAmount;
            transform.localScale = originScale * scale;
        }
    }
}

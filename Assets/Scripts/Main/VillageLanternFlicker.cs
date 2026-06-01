using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Realistic lantern-flicker effect for 2D village lanterns.
///
/// Attach to a GameObject that has (or whose children have) a Light2D component.
/// The script drives the light's intensity with layered Perlin noise so the
/// flicker feels organic rather than mechanical.
///
/// Scene Setup:
///   - Attach to the root lantern GameObject (the one that also has
///     VillageDecorationBobber).
///   - The script auto-finds the first Light2D in its children.
/// </summary>
public class VillageLanternFlicker : MonoBehaviour
{
    [Header("Base Intensity")]
    [SerializeField] private float baseIntensity  = 0.9f;
    [SerializeField] private float flickerAmount  = 0.35f;   // max deviation from base

    [Header("Noise Speed")]
    [SerializeField] private float fastSpeed  = 6.0f;   // rapid micro-flicker
    [SerializeField] private float slowSpeed  = 1.2f;   // slow swell

    [Header("Rare Dip")]
    [Tooltip("Chance per second that the lantern briefly dims sharply.")]
    [SerializeField] private float dipChance      = 0.04f;
    [SerializeField] private float dipIntensity   = 0.25f;
    [SerializeField] private float dipDuration    = 0.08f;

    // ── Private ───────────────────────────────────────────────────────────────
    private Light2D targetLight;
    private float   noiseOffset;
    private float   dipTimer;
    private bool    isDipping;

    // ── Unity ─────────────────────────────────────────────────────────────────

    private void Awake()
    {
        // Search self first, then children
        targetLight = GetComponent<Light2D>() ?? GetComponentInChildren<Light2D>();
        noiseOffset = Random.Range(0f, 100f);
    }

    private void Update()
    {
        if (targetLight == null) return;

        HandleDip();

        if (isDipping) return;

        float t    = Time.time + noiseOffset;
        float fast = Mathf.PerlinNoise(t * fastSpeed, noiseOffset)          * 2f - 1f;
        float slow = Mathf.PerlinNoise(t * slowSpeed, noiseOffset + 50f)    * 2f - 1f;
        float noise = (fast * 0.6f + slow * 0.4f) * flickerAmount;

        targetLight.intensity = Mathf.Max(0f, baseIntensity + noise);
    }

    private void HandleDip()
    {
        if (isDipping)
        {
            dipTimer -= Time.deltaTime;
            if (dipTimer <= 0f)
            {
                isDipping = false;
                targetLight.intensity = baseIntensity;
            }
            else
            {
                targetLight.intensity = dipIntensity;
            }
            return;
        }

        // Roll for a dip this frame
        if (Random.value < dipChance * Time.deltaTime)
        {
            isDipping = true;
            dipTimer  = dipDuration;
        }
    }
}

using UnityEngine;

/// <summary>
/// Spawns and configures ambient particle effects that bring the Village Hub to life.
///
/// Two independent emitters are created at runtime:
///   - Fireflies  : slow, gently-glowing motes that drift upward
///   - Petals     : soft pink/white flakes that drift diagonally downward
///
/// Scene Setup:
///   - Attach to any always-active GameObject in the hub (e.g. HubGameManager or a dedicated
///     "VillageAmbience" empty).
///   - Tweak colours, counts, and spawn area in the Inspector.
///   - Both emitters parent themselves under this transform so the hierarchy stays clean.
/// </summary>
public class VillageAmbience : MonoBehaviour
{
    // ── Fireflies ──────────────────────────────────────────────────────────────

    [Header("Fireflies")]
    [SerializeField] private bool showFireflies = true;
    [SerializeField] private Color fireflyColor = new Color(0.85f, 1f, 0.4f, 0.9f);
    [SerializeField] private int fireflyCount = 18;
    [SerializeField] private Vector2 fireflySpawnArea = new Vector2(14f, 8f);
    [SerializeField] private float fireflyMinSize = 0.04f;
    [SerializeField] private float fireflyMaxSize = 0.12f;

    // ── Petals ─────────────────────────────────────────────────────────────────

    [Header("Petals")]
    [SerializeField] private bool showPetals = true;
    [SerializeField] private Color petalColor = new Color(1f, 0.78f, 0.85f, 0.8f);
    [SerializeField] private int petalCount = 24;
    [SerializeField] private Vector2 petalSpawnArea = new Vector2(18f, 2f);
    [SerializeField] private float petalMinSize = 0.06f;
    [SerializeField] private float petalMaxSize = 0.15f;

    // ── Unity ──────────────────────────────────────────────────────────────────

    private void Start()
    {
        if (showFireflies) CreateFireflies();
        if (showPetals)    CreatePetals();
    }

    // ── Emitter builders ───────────────────────────────────────────────────────

    private void CreateFireflies()
    {
        GameObject go = new GameObject("Fireflies");
        go.transform.SetParent(transform, false);

        ParticleSystem ps = go.AddComponent<ParticleSystem>();

        // Stop so we can configure before playing
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.loop             = true;
        main.startLifetime    = new ParticleSystem.MinMaxCurve(3f, 6f);
        main.startSpeed       = new ParticleSystem.MinMaxCurve(0.02f, 0.08f);
        main.startSize        = new ParticleSystem.MinMaxCurve(fireflyMinSize, fireflyMaxSize);
        main.startColor       = fireflyColor;
        main.maxParticles     = fireflyCount * 2;
        main.simulationSpace  = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = fireflyCount / 4f;

        var shape = ps.shape;
        shape.enabled     = true;
        shape.shapeType   = ParticleSystemShapeType.Rectangle;
        shape.scale       = new Vector3(fireflySpawnArea.x, fireflySpawnArea.y, 1f);

        // Drift gently upward with slight side-wander
        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space   = ParticleSystemSimulationSpace.World;
        vel.x       = new ParticleSystem.MinMaxCurve(-0.05f, 0.05f);
        vel.y       = new ParticleSystem.MinMaxCurve(0.02f, 0.07f);

        // Twinkle: fade in then out
        var col = ps.colorOverLifetime;
        col.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(fireflyColor, 0f),
                new GradientColorKey(fireflyColor, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0f,   0f),
                new GradientAlphaKey(1f,   0.3f),
                new GradientAlphaKey(0.6f, 0.65f),
                new GradientAlphaKey(1f,   0.8f),
                new GradientAlphaKey(0f,   1f)
            }
        );
        col.color = grad;

        // Renderer: use a default circle/dot sprite
        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material   = CreateUnlitMaterial(fireflyColor);
        renderer.sortingLayerName = "Default";
        renderer.sortingOrder     = 5;

        ps.Play();
    }

    private void CreatePetals()
    {
        GameObject go = new GameObject("Petals");
        go.transform.SetParent(transform, false);
        // Spawn above the visible area so petals fall into view
        go.transform.localPosition = new Vector3(0f, 5f, 0f);

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.loop            = true;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(4f, 7f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(0.5f, 1.2f);
        main.startSize       = new ParticleSystem.MinMaxCurve(petalMinSize, petalMaxSize);
        main.startColor      = petalColor;
        main.startRotation   = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
        main.maxParticles    = petalCount * 2;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 0.06f;

        var emission = ps.emission;
        emission.rateOverTime = petalCount / 5f;

        var shape = ps.shape;
        shape.enabled   = true;
        shape.shapeType = ParticleSystemShapeType.Rectangle;
        shape.scale     = new Vector3(petalSpawnArea.x, petalSpawnArea.y, 1f);

        // Drift diagonally
        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space   = ParticleSystemSimulationSpace.World;
        vel.x       = new ParticleSystem.MinMaxCurve(-0.3f, 0.15f);
        vel.y       = new ParticleSystem.MinMaxCurve(-1.1f, -0.4f);

        // Gentle spin
        var rot = ps.rotationOverLifetime;
        rot.enabled = true;
        rot.z       = new ParticleSystem.MinMaxCurve(-60f * Mathf.Deg2Rad, 60f * Mathf.Deg2Rad);

        // Fade out near the end of lifetime
        var col = ps.colorOverLifetime;
        col.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(petalColor, 0f),
                new GradientColorKey(petalColor, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(1f, 0.1f),
                new GradientAlphaKey(1f, 0.75f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        col.color = grad;

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode       = ParticleSystemRenderMode.Billboard;
        renderer.material         = CreateUnlitMaterial(petalColor);
        renderer.sortingLayerName = "Default";
        renderer.sortingOrder     = 5;

        ps.Play();
    }

    // ── Utility ────────────────────────────────────────────────────────────────

    private Material CreateUnlitMaterial(Color color)
    {
        // Sprites/Default is available in every Unity project
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = color;
        return mat;
    }
}

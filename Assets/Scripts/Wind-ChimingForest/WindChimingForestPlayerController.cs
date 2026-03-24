using UnityEngine;

/// <summary>
/// Player controller for the Wind-Chiming Forest trial.
/// Extends PlayerMovement — only overrides HandleInput and ApplyMovement.
///
/// Movement model:
///   - The player constantly scrolls upward at scrollSpeed (matching the leaves).
///   - Dash + a horizontal direction (left/right WASD) sidesteps to the adjacent lane.
///   - The player snaps smoothly to the lane's X position.
///   - No left/right freeform movement — all lateral movement is lane-based via Dash.
///
/// Inspector Setup:
///   - Attach to the Player root alongside PlayerInput and Rigidbody2D (already required by base).
///   - Rigidbody2D: Gravity Scale = 0, Freeze Rotation Z, Body Type = Dynamic.
///   - Add a small Collider2D (e.g. CircleCollider2D, NOT a trigger) for physics,
///     and a second small Collider2D set to Is Trigger = true for leaf detection.
///     OR: Use one trigger Collider2D — the leaf detection uses triggers anyway.
///   - Tag the Player "Player".
///   - Lane X Positions: match the values you use in WindChimingForestGameManager.
///   - Scroll Speed: must match WindChimingForestGameManager's scrollSpeed exactly.
///   - The base class 'speed' field is unused here — you can leave it at its default.
/// </summary>
public class WindChimingForestPlayerController : PlayerMovement
{
    [Header("Lane Settings")]
    [SerializeField] private float[] laneXPositions = { -2f, 0f, 2f };
    [SerializeField] private int startLaneIndex = 1; // 0 = left, 1 = middle, 2 = right
    [SerializeField] private float laneSnapSpeed = 15f; // How fast player lerps to lane X

    [Header("Dash")]
    [SerializeField] private float dashCooldown = 0.35f; // Minimum time between lane switches

    [Header("Scroll")]
    [Tooltip("Must match the scrollSpeed in WindChimingForestGameManager.")]
    [SerializeField] public float scrollSpeed = 2f;

    public int CurrentLaneIndex { get; private set; }
    private float dashTimer = 0f;

    protected override void Start()
    {
        base.Start();
        CurrentLaneIndex = startLaneIndex;

        // Immediately snap to starting lane — no lerp at spawn
        Vector3 pos = transform.position;
        pos.x = laneXPositions[CurrentLaneIndex];
        transform.position = pos;
    }

    /// <summary>
    /// Checks for Dash input and switches lanes if a valid horizontal direction is held.
    /// Called every Update by the base class — button presses are safe here.
    /// </summary>
    protected override void HandleInput()
    {
        dashTimer -= Time.deltaTime;
        if (!canMove || dashTimer > 0f) return;

        if (!playerInput.obtainMoveInputActions().dash) return;

        // Use the move vector's X axis to determine which direction to dash
        float horizontal = moveValue.x;

        if (horizontal > 0.1f)
            TryChangeLane(CurrentLaneIndex + 1);
        else if (horizontal < -0.1f)
            TryChangeLane(CurrentLaneIndex - 1);
    }

    private void TryChangeLane(int targetIndex)
    {
        if (targetIndex < 0 || targetIndex >= laneXPositions.Length) return;
        CurrentLaneIndex = targetIndex;
        dashTimer = dashCooldown;
    }

    /// <summary>
    /// Replaces base ApplyMovement entirely:
    ///   X: smooth lerp toward current lane's X position.
    ///   Y: constant upward scroll (matches leaf scroll speed).
    /// </summary>
    protected override void ApplyMovement()
    {
        if (!canMove)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float targetX = laneXPositions[CurrentLaneIndex];
        float newX    = Mathf.Lerp(transform.position.x, targetX, laneSnapSpeed * Time.fixedDeltaTime);
        float xVel    = (newX - transform.position.x) / Time.fixedDeltaTime;

        rb.linearVelocity = new Vector2(xVel, scrollSpeed);
    }

    /// <summary>
    /// Called by WindChimingForestGameManager on respawn.
    /// Teleports to the given position and resets to the given lane.
    /// </summary>
    public void RespawnAt(Vector3 position, int laneIndex)
    {
        transform.position = position;
        CurrentLaneIndex   = Mathf.Clamp(laneIndex, 0, laneXPositions.Length - 1);
        dashTimer          = 0f;
        rb.linearVelocity  = Vector2.zero;
    }
}
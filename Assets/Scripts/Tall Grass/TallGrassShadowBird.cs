using UnityEngine;

/// <summary>
/// A Shadow Bird that patrols a set of waypoints and detects the player.
///
/// PATROLLING: Moves between Transform[] waypoints in a looping sequence.
///             Uses MoveTowards for smooth, deterministic movement.
///
/// DETECTION:  Each FixedUpdate, checks if the player is within detectionRadius
///             AND the player is not hidden (IsHidden == false).
///             If so, notifies the TallGrassGameManager rather than handling the
///             consequences itself. This keeps all game-state logic centralized.
///
/// ACTIVATION: Does nothing until SetActive(true) is called by the Manager.
///             This prevents birds from patrolling or detecting during intro sequences.
/// </summary>
public class TallGrassShadowBird : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector Fields
    // -------------------------------------------------------------------------

    [Header("Patrol Settings")]
    [Tooltip("The ordered list of positions this bird travels between. Loops.")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float patrolSpeed = 3f;

    [Header("Detection")]
    [SerializeField] private float detectionRadius = 2.5f;

    [Header("References")]
    [SerializeField] private TallGrassGameManager gameManager;

    // -------------------------------------------------------------------------
    // Private State
    // -------------------------------------------------------------------------

    private int currentWaypointIndex = 0;
    private bool isActive = false;
    private bool hasFiredCapture = false; // Prevents spamming the capture event

    // Cached reference to the player (set by Manager via SetPlayerReference)
    private TallGrassPlayerController player;

    // -------------------------------------------------------------------------
    // Public API (called by Manager)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Must be called by TallGrassGameManager before the bird will do anything.
    /// </summary>
    public void Activate(TallGrassPlayerController playerRef)
    {
        player = playerRef;
        isActive = true;
        hasFiredCapture = false;

        // Start at the nearest waypoint to avoid snapping across the map
        SnapToNearestWaypoint();
    }

    /// <summary>
    /// Freezes the bird in place. Called during respawn or game pause.
    /// </summary>
    public void Deactivate()
    {
        isActive = false;
    }

    /// <summary>
    /// Resets the bird's capture flag after the player has respawned.
    /// Called by the Manager so the bird can detect again.
    /// </summary>
    public void ResetCaptureFlag()
    {
        hasFiredCapture = false;
    }

    // -------------------------------------------------------------------------
    // Lifecycle
    // -------------------------------------------------------------------------

    void FixedUpdate()
    {
        if (!isActive) return;

        Patrol();
        CheckDetection();
    }

    // -------------------------------------------------------------------------
    // Private — Patrol Logic
    // -------------------------------------------------------------------------

    private void Patrol()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform target = waypoints[currentWaypointIndex];
        transform.position = Vector2.MoveTowards(transform.position, target.position, patrolSpeed * Time.fixedDeltaTime);

        // Advance to next waypoint when close enough
        if (Vector2.Distance(transform.position, target.position) < 0.05f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }

    private void SnapToNearestWaypoint()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        float nearest = float.MaxValue;
        for (int i = 0; i < waypoints.Length; i++)
        {
            float dist = Vector2.Distance(transform.position, waypoints[i].position);
            if (dist < nearest)
            {
                nearest = dist;
                currentWaypointIndex = i;
            }
        }
    }

    // -------------------------------------------------------------------------
    // Private — Detection Logic
    // -------------------------------------------------------------------------

    private void CheckDetection()
    {
        if (player == null || hasFiredCapture) return;

        float distToPlayer = Vector2.Distance(transform.position, player.transform.position);
        bool inRange = distToPlayer <= detectionRadius;
        bool exposed = !player.IsHidden;

        if (inRange && exposed)
        {
            hasFiredCapture = true; // Prevent repeated calls until respawn
            gameManager.OnPlayerCaptured();
        }
    }

    // -------------------------------------------------------------------------
    // Gizmo
    // -------------------------------------------------------------------------

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.8f, 0f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        if (waypoints == null) return;
        Gizmos.color = Color.yellow;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            Gizmos.DrawSphere(waypoints[i].position, 0.15f);
            int next = (i + 1) % waypoints.Length;
            if (waypoints[next] != null)
                Gizmos.DrawLine(waypoints[i].position, waypoints[next].position);
        }
    }
}
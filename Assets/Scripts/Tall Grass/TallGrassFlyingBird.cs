using UnityEngine;

/// <summary>
/// A Shadow Bird that patrols between a set of waypoints.
/// If the player is inside the bird's trigger zone while NOT hidden, they are caught.
///
/// Inspector Setup:
///   - Attach to a bird GameObject with a SpriteRenderer and Collider2D (Is Trigger = true)
///   - Create several empty GameObjects in the scene as waypoints (e.g. "Bird1_WP_A", "Bird1_WP_B")
///   - Drag those waypoints into the Waypoints array
///   - The bird patrols them in order and loops back
///   - Size the trigger Collider2D to match the bird's visual detection zone
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class TallGrassFlyingBird : MonoBehaviour
{
    [Header("Patrol")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float speed = 3f;
    [SerializeField] private float waypointReachDistance = 0.1f;

    [Header("Visual")]
    [SerializeField] private bool flipSpriteOnDirection = true;

    private int currentWaypointIndex = 0;
    private TallGrassGameManager gameManager;

    void Start()
    {
        GetComponent<Collider2D>().isTrigger = true;

        gameManager = FindFirstObjectByType<TallGrassGameManager>();
        if (gameManager == null)
            Debug.LogWarning("[TallGrassFlyingBird] No TallGrassGameManager found in scene.");
    }

    void Update()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        Patrol();
    }

    private void Patrol()
    {
        Transform target = waypoints[currentWaypointIndex];
        Vector2 directionToTarget = target.position - transform.position;

        transform.position = Vector2.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        // Flip sprite to face movement direction
        if (flipSpriteOnDirection && directionToTarget.x != 0f)
        {
            Vector3 scale = transform.localScale;
            scale.x = directionToTarget.x < 0f
                ? -Mathf.Abs(scale.x)
                :  Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

        if (Vector2.Distance(transform.position, target.position) < waypointReachDistance)
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        TallGrassPlayerController player = other.GetComponent<TallGrassPlayerController>();
        if (player != null && !player.isHidden)
            gameManager?.PlayerCaught();
    }
}
using UnityEngine;

public class FlyingBird : MonoBehaviour
{
    public Transform[] waypoints; // Points the bird flies between
    public float speed = 3f;
    private int currentPoint = 0;

    void Update() {
        // Patrol Logic
        transform.position = Vector2.MoveTowards(transform.position, waypoints[currentPoint].position, speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, waypoints[currentPoint].position) < 0.1f) {
            currentPoint = (currentPoint + 1) % waypoints.Length;
        }
    }

    void OnTriggerStay2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            // Access the Player's script to check if they are hidden
            PlayerControllerTallGrass player = other.GetComponent<PlayerControllerTallGrass>();

            if (!player.isHidden) {
                Debug.Log("Caught by the Shadow Bird!");
                // Restart Level or Reset Position
                UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            }
        }
    }
}

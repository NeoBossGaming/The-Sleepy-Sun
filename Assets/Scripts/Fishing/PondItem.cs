using UnityEngine;

public class PondItem: MonoBehaviour {
    public float speed = 5f;

    void Update() {
        transform.Translate(Vector3.right * speed * Time.deltaTime);
        
        // Destroy if it goes off-screen to save memory (like clearing a buffer)
        if (transform.position.x > 10) Destroy(gameObject);
    }
}
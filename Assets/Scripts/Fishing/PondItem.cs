using UnityEngine;

public class PondItem: MonoBehaviour {
    public float speed = 5f;
    public float speedDeviation = 1f;
    public float sizeDeviation = 0.1f;
    private void Start() {
        speed = UnityEngine.Random.Range(speed - speedDeviation, speed + speedDeviation);
        float sizeMultiplier = UnityEngine.Random.Range(1f-sizeDeviation, 1f+sizeDeviation);
        transform.localScale = new Vector3(sizeMultiplier, sizeMultiplier, sizeMultiplier);
        Debug.Log(transform.scale);
    }
    void Update() {
        transform.Translate(Vector3.right * speed * Time.deltaTime);
        
        // Destroy if it goes off-screen to save memory (like clearing a buffer)
        if (transform.position.x > 10) Destroy(gameObject);
    }
}
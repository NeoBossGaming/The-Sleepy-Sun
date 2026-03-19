using UnityEngine;

public class FishingSwimmingItem : MonoBehaviour
{
    [Header("Object Properties")]
    [SerializeField] private float objectSwimSpeed = 5f;
    [SerializeField] private float objectSwimSpeedDeviation = 1f;
    [SerializeField] private float objectSizeDeviation = 0.1f;
    [SerializeField] private bool correctObject;

    private bool capturedByReel = false;
    
    private void Start() {
        // Set initial state of object
        capturedByReel = false;

        // Mutate object slightly
        objectSwimSpeed = UnityEngine.Random.Range(objectSwimSpeed - objectSwimSpeedDeviation, objectSwimSpeed + objectSwimSpeedDeviation);
        float sizeMultiplier = UnityEngine.Random.Range(1f - objectSizeDeviation, 1f + objectSizeDeviation);
        transform.localScale = new Vector3(sizeMultiplier, sizeMultiplier, sizeMultiplier);
    }
    void Update() {
        if (!capturedByReel) transform.Translate(Vector3.right * objectSwimSpeed * Time.deltaTime);
        
        // Destroy if it goes off-screen to save memory
        if (transform.position.x > 10) Destroy(gameObject);
    }
    
    /// <summary>
    /// Stop all moving actions, and start to follow reel.
    /// Called when the reel hit this object.
    /// </summary>
    public bool StopAllActions()
    {
        capturedByReel = true;
        return correctObject;
    }
}

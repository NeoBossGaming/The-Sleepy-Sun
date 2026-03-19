using UnityEngine;

public class FishingSwimmingItem : MonoBehaviour
{
    [SerializeField] private bool correctObject;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Stop all moving actions, and start to follow reel.
    /// Called when the reel hit this object.
    /// </summary>
    private void StopAllActions()
    {
        return correctObject;
    }
}

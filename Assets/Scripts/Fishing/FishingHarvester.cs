using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(PlayerInput))]
public class FishingHarvester : MonoBehaviour
{
    [SerializeField] private FishingGameManager gameManager;
    [SerializeField] private PlayerInput playerInputScript;
    [SerializeField] private bool isReeling;

    [Header("Fishing Settings")]
    [SerializeField] private Transform reelObject;
    [SerializeField] private float fishingReelingSpeed = 5f;
    [SerializeField] private Transform reelEndTransform;
    [SerializeField] private Transform reelStartTransform;
    [SerializeField] private Vector2 reelDetectionBoxSize = new Vector2(0.5f, 1.0f);

    private List<FishingSwimmingItem> itemsCaught;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerInputScript = GetComponent<PlayerInput>();
    }

    // Update is called once per frame
    void Update()
    {
        bool initiateReelAction = playerInputScript.obtainMoveInputActions().dash;
        if (initiateReelAction && !isReeling)
        {
            StartCoroutine(ReelRoutine());    
        }
    }

    /// <summary>
    /// The entire action of running the reeling animation and the process of reeling.
    /// </summary>
    IEnumerator ReelRoutine()
    {
        isReeling = true;
        // Glide the reel down towards the end.
        while (Vector3.Distance(reelObject.position, reelEndTransform.position) > 0.1f)
        {
            reelObject.position = Vector3.MoveTowards(reelObject.position, reelEndTransform.position, fishingReelingSpeed * Time.deltaTime);
            
            // Capture all available objects.
            CaptureAllCollidingFishingObject();
            
            yield return null;
        }
        
        DestoryAllCapturedObjects(); // Clears all of the captured objects once the reel has return

        // Glide the reel up towards the start
        while (Vector3.Distance(reelObject.position, reelEndTransform.position) > 0.1f)
        {
            reelObject.position = Vector3.MoveTowards(reelObject.position, reelStartTransform.position, fishingReelingSpeed * Time.deltaTime);
            
            // Capture all available objects.
            CaptureAllCollidingFishingObject();
            
            yield return null;
        }

        isReeling = false;
    }

    /// <summary>
    /// Check any colliding objects with the reel.
    /// If there are, check if those objects have the FishingSwimmingItem script.
    /// If yes, suspend all actions and add score based on object's value through
    /// the manager script.
    /// </summary>
    private void CaptureAllCollidingFishingObject()
    {
        // List off all the objects the reel is colliding with
        Collider2D[] collidingObjects = Physics2D.OverlapBoxAll(reelObject.position, reelDetectionBoxSize, 0f);
        foreach (Collider2D col in collidingObjects)
        {
            FishingSwimmingItem objectScript = col.gameObject.GetComponent<FishingSwimmingItem>();
            if (objectScript != null)
            {
                if (objectScript.StopAllActions()) gameManager.capturedObject(true);
                else gameManager.capturedObject(false);
                itemsCaught.Add(objectScript);
            }

        }
    }

    /// <summary>
    /// Called when the reel has reached the finishing point.
    /// Clear and destroy's all of the captured objects.
    /// </summary>
    private void DestoryAllCapturedObjects()
    {
        foreach (FishingSwimmingItem item in itemsCaught)
        {
            Destory(item.gameObject);
        }
    }
}

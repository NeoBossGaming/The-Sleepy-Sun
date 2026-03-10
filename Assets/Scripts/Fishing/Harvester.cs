using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;


public class Harvester : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    public int score = 0;
    public Transform firePoint; 
    public Transform targetPoint; // The end point where the reel glides to
    public Vector2 boxSize = new Vector2(0.5f, 1.0f); // Size of the detection box
    public float reelSpeed = 5f;
    
    private bool isReeling = false;
    private Vector3 originalPosition;
    private List<GameObject> caughtObjects = new List<GameObject>();

    void Start()
    {
        // Remember where the firePoint starts
        if (firePoint != null)
        {
            originalPosition = firePoint.localPosition;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isReeling)
        {
            StartCoroutine(ReelRoutine());
        }
        scoreText.text = $"Score: {score}";
    }

    IEnumerator ReelRoutine()
    {
        isReeling = true;

        // 1. GLIDE DOWN
        while (Vector3.Distance(firePoint.position, targetPoint.position) > 0.1f)
        {
            firePoint.position = Vector3.MoveTowards(firePoint.position, targetPoint.position, reelSpeed * Time.deltaTime);
            
            // Check for objects while moving down
            CheckForCollisions();
            
            // Make caught objects follow the firePoint
            UpdateCaughtObjectsPosition();
            
            yield return null;
        }
        foreach (GameObject obj in caughtObjects)
        {
            if (obj != null)
            {
                if (obj.CompareTag("Fish")) score += 10;
                else if (obj.CompareTag("Plastic")) score -= 50;
                
                Destroy(obj);
            }
        }
        // 2. GLIDE BACK UP
        Vector3 worldStartPosition = transform.TransformPoint(originalPosition);
        while (Vector3.Distance(firePoint.position, worldStartPosition) > 0.1f)
        {
            firePoint.position = Vector3.MoveTowards(firePoint.position, worldStartPosition, reelSpeed * Time.deltaTime);
            
            // Keep updating caught objects so they "stick" to the reel
            UpdateCaughtObjectsPosition();
            
            yield return null;
        }

        // 3. PROCESS CAUGHT OBJECTS (Once back at the start)

        
        caughtObjects.Clear();
        firePoint.localPosition = originalPosition; // Snap back to exact start
        isReeling = false;
    }

    void CheckForCollisions()
    {
        Collider2D[] hitObjects = Physics2D.OverlapBoxAll(firePoint.position, boxSize, 0f);
        foreach (Collider2D col in hitObjects)
        {
            // If it's a fish or plastic and we haven't caught it yet
            if ((col.CompareTag("Fish") || col.CompareTag("Plastic")) && !caughtObjects.Contains(col.gameObject))
            {
                caughtObjects.Add(col.gameObject);
                
                // Disable the collider so it doesn't trigger other things while being reeled
                col.enabled = false; 
            }
        }
    }

    void UpdateCaughtObjectsPosition()
    {
        foreach (GameObject obj in caughtObjects)
        {
            if (obj != null)
            {
                obj.transform.position = firePoint.position;
            }
        }
    }

    // GIZMOS FOR DEBUGGING
    void OnDrawGizmos()
    {
        if (firePoint != null)
        {
            Gizmos.color = Color.green;
            // Draw the detection box
            Gizmos.DrawWireCube(firePoint.position, new Vector3(boxSize.x, boxSize.y, 0.1f));
            
            if (targetPoint != null)
            {
                Gizmos.color = Color.red;
                // Draw a line showing the path of the reel
                Gizmos.DrawLine(firePoint.position, targetPoint.position);
            }
        }
    }
}
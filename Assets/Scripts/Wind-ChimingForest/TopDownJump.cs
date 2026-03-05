using UnityEngine;
using System.Collections;

public class TopDownJump : MonoBehaviour
{
    [Header("Jump Settings")]
    [SerializeField] private float jumpDuration = 0.6f;
    [SerializeField] private float jumpHeightMax = 1.5f;
    
    [Header("Detection + Jump System")]
    [SerializeField] private float leafDetectionRange = 2.0f;
    [SerializeField] private string leafTag;
    [SerializeField] private RhythmPlayerController playerController;
    [SerializeField] private JumpAnchor targetedLeafAnchor;
    [SerializeField] public JumpAnchor currentLeafAnchor;
    [SerializeField] private float leafYOffset;


    [Header("Visuals")]
    // Since you don't have a child, we'll move the SpriteRenderer component's offset
    [SerializeField] private SpriteRenderer characterSprite;

    private float playerDirection;
    public bool isBusy = false;

    private void Start() {
        if (playerController == null) playerController = GetComponent<RhythmPlayerController>();
    }

    private void Update() {
        if (isBusy) return; // Don't look for anchors or take input while mid-air
        checkForAnchor();
        if (playerController.getMoveValue()[0] != 0.0f) playerDirection = Mathf.Sign(playerController.getMoveValue()[0]);
        // TRIGGER THE JUMP
        if (playerController.getJumpValue() && targetedLeafAnchor != null)
        {
            StartCoroutine(ExecuteLeap(targetedLeafAnchor.transform.position, targetedLeafAnchor));
        }
    }

    private void checkForAnchor()
    {
        Vector2 detectionCenter = new Vector2 (transform.position.x + (leafDetectionRange * playerDirection), transform.position.y);
        Collider2D leafDetected = Physics2D.OverlapCircle(detectionCenter, leafDetectionRange / 2);
        if (leafDetected != null && leafDetected.gameObject.CompareTag(leafTag))
        {
            targetedLeafAnchor = leafDetected.GetComponent<JumpAnchor>();
        }
        else
        {
            targetedLeafAnchor = null;
        }
    }
    IEnumerator ExecuteLeap(Vector3 targetPos, JumpAnchor target)
    {
        currentLeafAnchor = null;
        // 1. Define the OFFSET target immediately
        Vector3 newTargetPos = new Vector3(targetPos.x, targetPos.y + leafYOffset, targetPos.z);
        
        isBusy = true;   
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        Vector3 startPos = transform.position;

        RigidbodyType2D originalType = rb.bodyType;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;

        playerController.isStationary = false;
        playerController.ableToLandMove = false;

        float elapsed = 0;
        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / jumpDuration;

            // Use the OFFSET position for the entire Lerp
            transform.position = Vector3.Lerp(startPos, newTargetPos, percent);

            float arc = jumpHeightMax * 4 * (percent * (1 - percent));
            // Keep X and Z local positions, only change Y for the arc
            characterSprite.transform.localPosition = new Vector3(characterSprite.transform.localPosition.x, arc, characterSprite.transform.localPosition.z);

            yield return null;
        }

        // 2. FINAL SNAP (Ensure everything lands at the offset)
        transform.position = newTargetPos;
        currentLeafAnchor = target;

        // IMPORTANT: Reset the sprite height to 0 so she isn't floating        
        rb.bodyType = originalType;
        playerController.isStationary = false;
        playerController.ableToLandMove = true;
        isBusy = false;
    }

    // Visual aid in the Editor to see the detection zone
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector2 detectionCenter = (Vector2)transform.position + new Vector2(playerDirection * leafDetectionRange, 0);
        Gizmos.DrawWireSphere(detectionCenter, leafDetectionRange/2);
    }

}
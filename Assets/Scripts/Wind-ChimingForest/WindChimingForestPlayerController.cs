using UnityEngine;
using System.Collections;

public class WindChimingForestPlayerController : PlayerMovement
{
    [Header("Dash Settings")]
    [SerializeField] private float dashDuration = 0.18f;
    [SerializeField] private float detectionDistance = 3f;
    [SerializeField] private Vector2 detectionBoxSize = new Vector2(0.4f, 0.8f);

    private WindChimingForestLeaf currentLeaf;
    private int dashDirection = 1;
    private bool isDashing = false;

    public WindChimingForestLeaf CurrentLeaf => currentLeaf;

    public void SetCurrentLeaf(WindChimingForestLeaf leaf)
    {
        if (currentLeaf != null) currentLeaf.SetHasPlayer(false);
        currentLeaf = leaf;

        if (currentLeaf != null)
        {
            currentLeaf.SetHasPlayer(true);
            transform.position = currentLeaf.transform.position;
        }
    }

    protected override void HandleInput()
    {
        if (!canMove || isDashing) return;

        if (moveValue.x < -0.3f) dashDirection = -1;
        else if (moveValue.x > 0.3f) dashDirection = 1;

        if (playerInput.obtainMoveInputActions().dash)
        {
            TryDash();
        }
    }

    // Switched to a standard Update logic or called manually to sync with scroll
    void LateUpdate()
    {
        if (!canMove) return;

        rb.linearVelocity = Vector2.zero;

        if (!isDashing && currentLeaf != null && currentLeaf.IsActive)
        {
            transform.position = currentLeaf.transform.position;
        }
    }

    protected override void ApplyMovement() 
    {
        // Left empty because we are using LateUpdate for scroll-syncing
    }

    private void TryDash()
    {
        WindChimingForestLeaf target = DetectLeafInDirection(dashDirection);
        if (target == null) return;

        StartCoroutine(DashRoutine(target));
    }

    private WindChimingForestLeaf DetectLeafInDirection(int direction)
    {
        Vector2 boxCenter = (Vector2)transform.position + Vector2.right * direction * detectionDistance;
        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, detectionBoxSize, 0f);

        WindChimingForestLeaf closest = null;
        float closestDist = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            WindChimingForestLeaf leaf = hit.GetComponent<WindChimingForestLeaf>();
            if (leaf == null || !leaf.IsActive) continue;

            float dist = Mathf.Abs(hit.transform.position.x - transform.position.x);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = leaf;
            }
        }
        return closest;
    }

    private IEnumerator DashRoutine(WindChimingForestLeaf targetLeaf)
    {
        isDashing = true;
        if (currentLeaf != null) currentLeaf.SetHasPlayer(false);
        currentLeaf = null;

        float startX = transform.position.x;
        float elapsed = 0f;

        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / dashDuration);

            // Sync with target leaf's current world position
            transform.position = new Vector3(
                Mathf.Lerp(startX, targetLeaf.transform.position.x, t),
                targetLeaf.transform.position.y,
                transform.position.z
            );

            yield return null;
        }

        transform.position = targetLeaf.transform.position;
        currentLeaf = targetLeaf;
        currentLeaf.SetHasPlayer(true);
        isDashing = false;
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 boxCenter = (Vector2)transform.position + Vector2.right * dashDirection * detectionDistance;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(boxCenter, detectionBoxSize);
    }
}
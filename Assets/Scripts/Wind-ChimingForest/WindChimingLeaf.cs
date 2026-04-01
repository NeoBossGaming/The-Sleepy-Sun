using UnityEngine;
using System.Collections;

public class WindChimingLeaf : MonoBehaviour
{

    [Header("Visuals")]
    [SerializeField] private Transform leafVisualTransform; // The new child object
    [SerializeField] private SpriteRenderer leafSprite;     // The sprite on the child object

    public bool IsSafe { get; private set; } = true;

    private float originalWorldY; 
    private float originalWorldX; 
    private WindChimingGameManager gameManager;

    private float xShakeOffset; 
    private bool  isShaking;

    private Transform occupant; 

    public void InitializeLeaf(WindChimingGameManager manager)
    {
        gameManager    = manager;
        originalWorldY = transform.position.y; 
        originalWorldX = transform.position.x;

        IsSafe       = true;
        xShakeOffset = 0f;
        isShaking    = false;
        occupant     = null;

        leafSprite.enabled = true;
        GetComponent<Collider2D>().enabled = true;
    }

    public void ResetLeaf(WindChimingGameManager manager)
    {
        StopAllCoroutines();

        gameManager  = manager;
        IsSafe       = true;
        xShakeOffset = 0f;
        isShaking    = false;
        occupant     = null;

        // Reset the visual child to center
        if (leafVisualTransform != null)
        {
            leafVisualTransform.localPosition = Vector3.zero;
        }

        leafSprite.enabled = true;
        GetComponent<Collider2D>().enabled = true;
    }

    void Update()
    {
        if (gameManager == null) return;

        // 1. The main parent object ONLY handles the vertical scroll
        float syncedY = originalWorldY + gameManager.CurrentScrollOffset;
        transform.position = new Vector3(originalWorldX, syncedY, transform.position.z);

        // 2. The child visual object ONLY handles the horizontal shake
        if (leafVisualTransform != null)
        {
            leafVisualTransform.localPosition = new Vector3(xShakeOffset, 0f, 0f);
        }
    }

    public void TriggerShake(float duration, float amount, float collapseTime)
    {
        if (isShaking) return;
        StartCoroutine(ShakeSequence(duration, amount, collapseTime));
    }

    IEnumerator ShakeSequence(float duration, float amount, float collapseTime)
    {
        isShaking = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            xShakeOffset = Random.Range(-amount, amount);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Add the visual reset code here if you applied the visual separation fix previously!
        xShakeOffset = 0f;

        IsSafe = false;
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled     = false;

        yield return new WaitForSeconds(collapseTime);

        IsSafe    = true;
        isShaking = false;
        GetComponent<SpriteRenderer>().enabled = true;
        GetComponent<Collider2D>().enabled     = true;
    }

    public void SetOccupant(Transform t) => occupant = t;
    public void ClearOccupant()          => occupant = null;
    public bool IsOccupied               => occupant != null;
}
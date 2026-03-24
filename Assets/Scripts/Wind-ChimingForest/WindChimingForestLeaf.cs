using UnityEngine;
using System.Collections;

public class WindChimingForestLeaf : MonoBehaviour
{
    private bool isActive = true;
    private bool hasPlayer = false;
    private bool isShaking = false; // New flag

    private SpriteRenderer spriteRenderer;
    private Collider2D col;
    private Vector3 originalLocalPosition;
    private Coroutine activeShakeCoroutine;

    public bool IsActive => isActive;
    public bool HasPlayer => hasPlayer;
    public bool IsShaking => isShaking;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    public void InitPosition()
    {
        originalLocalPosition = transform.localPosition;
    }

    public void SetHasPlayer(bool value)
    {
        hasPlayer = value;
    }

    public void TriggerShake(float shakeDuration, float disappearDelay, System.Action onDisappear = null)
    {
        if (!isActive || isShaking) return;

        isShaking = true;
        if (activeShakeCoroutine != null) StopCoroutine(activeShakeCoroutine);
        activeShakeCoroutine = StartCoroutine(ShakeSequence(shakeDuration, disappearDelay, onDisappear));
    }

    public void Reactivate()
    {
        isShaking = false;
        if (activeShakeCoroutine != null)
        {
            StopCoroutine(activeShakeCoroutine);
            activeShakeCoroutine = null;
        }

        transform.localPosition = originalLocalPosition;
        spriteRenderer.enabled = true;
        col.enabled = true;
        isActive = true;
    }

    private IEnumerator ShakeSequence(float shakeDuration, float disappearDelay, System.Action onDisappear)
    {
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            float xNoise = Random.Range(-0.08f, 0.08f);
            float yNoise = Random.Range(-0.03f, 0.03f);
            transform.localPosition = originalLocalPosition + new Vector3(xNoise, yNoise, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalLocalPosition;
        yield return new WaitForSeconds(disappearDelay);

        spriteRenderer.enabled = false;
        col.enabled = false;
        isActive = false;
        isShaking = false;

        activeShakeCoroutine = null;
        onDisappear?.Invoke();
    }
}
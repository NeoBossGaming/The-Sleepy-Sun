using UnityEngine;
using System.Collections;

public class RhythmLeaf : MonoBehaviour
{
    public bool isSafe = true;
    private Vector3 originalLocalPos;

    void Start() {
        originalLocalPos = transform.localPosition;
    }

    public void TriggerShake() {
        StartCoroutine(ShakeSequence());
    }

    IEnumerator ShakeSequence() {
        float elapsed = 0f;
        float duration = 1.0f; // How long it shakes before it "breaks"

        while (elapsed < duration) {
            float xOffset = Random.Range(-0.1f, 0.1f);
            transform.localPosition = originalLocalPos + new Vector3(xOffset, 0, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // LEAF FALLS/VANISHES
        transform.localPosition = originalLocalPos; // Reset pos
        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;
        isSafe = false;
        yield return new WaitForSeconds(1.0f); // Time it stays gone

        // LEAF RESPAWNS
        GetComponent<Renderer>().enabled = true;
        GetComponent<Collider2D>().enabled = true;
        isSafe = true;
    }
}

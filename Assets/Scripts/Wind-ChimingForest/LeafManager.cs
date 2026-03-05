using UnityEngine;
using UnityEngine.Events;
using System.Collections;


public class LeafManager : MonoBehaviour {
    [SerializeField] private GameObject leafPrefab;
    [SerializeField] private RhythmLeaf[] allLeaves; // Drag all 5-6 leaves in the row here
    [SerializeField] private float bpm = 120f;
    [SerializeField] private float riseSpeed = 2f;
    [SerializeField] private int difficulty = 4;
    private float beatTimer;
    private float secondsPerBeat;
    private int beatCount = 0;

    void Start() {
        secondsPerBeat = 60f / bpm;
    }

    void Update() {
        // Move the entire row up
        transform.Translate(Vector3.up * riseSpeed * Time.deltaTime);

        beatTimer += Time.deltaTime;
        if (beatTimer >= secondsPerBeat * 4) { // Every 4 beats, run a cycle
            StartCoroutine(RunSurvivalCycle());
            beatTimer = 0;
        }
    }

    IEnumerator RunSurvivalCycle() {
        // 1. Pick random leaves to shake
        beatCount += 1;
        for (int i = 0; i < difficulty; i++) 
        {
            int index = Random.Range(0, allLeaves.Length);
            allLeaves[index].TriggerShake();
        }

        yield return new WaitForSeconds(1.5f); // Wait for them to fall/respawn
    }
}

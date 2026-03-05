using UnityEngine;

public class Spawner : MonoBehaviour {
    public GameObject[] items; // Drop your Fish and Plastic prefabs here
    public float spawnRate = 2f;

    void Start() {
        InvokeRepeating("SpawnItem", 1f, spawnRate);
    }

    void SpawnItem() {
        int index = Random.Range(0, items.Length);
        // Spawn at the far left off-screen
        Instantiate(items[index], new Vector3(-10, Random.Range(-1, 3.5f), 0), Quaternion.identity);
    }
}

using UnityEngine;

public class FishingGameManager : MonoBehaviour
{

    [Header("Game Settings")]
    [SerializeField] private int correctObjectDeltaScore = 1; // amount of increased score when capture correct object
    [SerializeField] private int wrongObjectDeltaScore = 5; // amount of decreased score when capture incorrect object

    [SerializeField] private float spawnObjectInterval; // interval time between spawning objects

    [Header("Spawn Objects")]
    [SerializeField] private GameObject[] spawnObjectLists; // list of gameobjects to be spawned randomly
    [SerializeField] private float xSpawnLocation; // x position of object spawn location
    [SerializeField] private float ySpawnLocationMin; // min y position of object spawn location
    [SerializeField] private float ySpawnLocationMax; // max y position of object spawn location
    private int score;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        score = 0;
        InvokeRepeating("SpawnItems", 1f, spawnObjectInterval);
    }

    /// <summary>
    /// Spawn the item's from the "spawnObjectLists" list in a random order.
    /// Initial spawn position will be in the most-left of the screen.
    /// </summary>
    private void SpawnItems()
    {
        // Select a random object from the list to be spawned.
        GameObject objectToBeSpawned = spawnObjectLists[Random.Range(0, spawnObjectLists.Length)];
        
        // Spawn items at left of screen
        Instantiate(objectToBeSpawned, new Vector3(xSpawnLocation, Random.Range(ySpawnLocationMin, ySpawnLocationMax), 0), Quaternion.identity);
    }

    /// <summary>
    /// Adds the player's score by a set variable.
    /// Called whenever the player captures a correct/wrong object.
    /// </summary>
    public void capturedObject(bool correctObject)
    {
        if (correctObject)
        {
            score += correctObjectDeltaScore;
        }
        else
        {
            score = Mathf.Max(0, score - wrongObjectDeltaScore);
        }
        
        Debug.Log("Score: " + score);
    }

}

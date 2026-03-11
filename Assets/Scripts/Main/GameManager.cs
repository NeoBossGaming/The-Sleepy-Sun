using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{


    [SerializeField] private bool hasSilverLeaf, hasCaveNectar, hasStardustFish, hasGoldenGinger;
    
    private bool hasAllWinConditions = false;
    
    void Awake() {
        // DontDestroyOnLoad(gameObject);
    }
    
    public void SetSilverLeaf(bool value) {
        hasSilverLeaf = value;
        CheckAllConditions();
    }

    public void SetCaveNectar(bool value) {
        hasCaveNectar = value;
        CheckAllConditions();
    }

    public void SetStardustFish(bool value) {
        hasStardustFish = value;
        CheckAllConditions();
    }

    public void SetGoldenGiner(bool value) {
        hasGoldenGinger = value;
        CheckAllConditions();
    }

    void CheckAllConditions() {
        hasAllWinConditions = hasSilverLeaf && hasCaveNectar && hasStardustFish && hasGoldenGinger;
    }

    public bool checkWinCondition()
    {
        return hasAllWinConditions;
    }

    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

}
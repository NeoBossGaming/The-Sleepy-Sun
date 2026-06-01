using UnityEngine;

public class GameManager : MonoBehaviour
{


    [SerializeField] private bool hasSilverLeaf, hasCaveNectar, hasStardustFish, hasGoldenGinger;
    
    private bool hasAllWinConditions = false;
    
    void Awake() {
        DontDestroyOnLoad(gameObject);
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

    public void SetGoldenGinger(bool value) {
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

    // Read-only accessors used by VillageNoticeboard to display ingredient progress
    public bool HasSilverLeaf  => hasSilverLeaf;
    public bool HasCaveNectar  => hasCaveNectar;
    public bool HasStardustFish => hasStardustFish;
    public bool HasGoldenGinger => hasGoldenGinger;

}
using UnityEngine;

/// <summary>
/// Place this on a trigger collider at the end of the path.
/// When the player walks into it, fires TriggerWin() on the TallGrassGameManager.
/// </summary>
public class TallGrassGoalZone : MonoBehaviour
{
    [SerializeField] private TallGrassGameManager gameManager;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            gameManager.TriggerWin();
        }
    }
}
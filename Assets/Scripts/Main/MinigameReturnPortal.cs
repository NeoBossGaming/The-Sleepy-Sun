using UnityEngine;

/// <summary>
/// Placed at the goal/exit of each minigame scene.
/// When the player enters this trigger after winning, return to the Hub.
///
/// Scene Setup:
/// - Add a Collider2D (Is Trigger) to the exit zone GameObject.
/// - Set hubSceneName in Inspector (e.g. "Scene_Hub").
/// - Only active after the minigame's GameManager signals win — 
///   call SetActive(true) from your minigame GameManager when the player wins.
/// </summary>
public class MinigameReturnPortal : MonoBehaviour
{
    [SerializeField] private string hubSceneName = "Scene_Hub";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        SceneTransitionManager.Instance.LoadScene(hubSceneName);
    }
}
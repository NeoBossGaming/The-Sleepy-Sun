using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central manager for the Village Hub scene.
/// 
/// Responsibilities:
/// - Holds references to all five path portals (four trials + Grand Teapot).
/// - Polls GameManager each frame to update which paths are unlocked.
/// - The Teapot path is locked until all four Wake-Up Brew ingredients are collected.
/// - Provides a single entry point (LoadScene) for portals to trigger scene transitions.
/// 
/// Scene Setup:
/// - Assign each HubPathPortal in the Inspector.
/// - The teapotPortal reference is used for the special lock check.
/// </summary>
public class HubGameManager : MonoBehaviour
{
    [Header("Path Portals")]
    [SerializeField] private HubPathPortal windChimePortal;
    [SerializeField] private HubPathPortal crystalCavesPortal;
    [SerializeField] private HubPathPortal starFishPondPortal;
    [SerializeField] private HubPathPortal tallGrassPortal;
    [SerializeField] private HubPathPortal grandTeapotPortal; // Locked until all ingredients gathered

    [Header("Feedback")]
    [SerializeField] private GameObject teapotLockedUI; // Optional: "You need all ingredients" message

    private GameManager gameManager;

    private void Start()
    {
        // GameManager uses DontDestroyOnLoad, so we find it in the scene
        gameManager = FindFirstObjectByType<GameManager>();

        if (gameManager == null)
        {
            Debug.LogWarning("HubGameManager: No GameManager found in scene. " +
                             "Make sure it persists from a previous scene or is placed in this one.");
        }

        // Trial paths are always accessible from the start of Chapter 2
        SetPortalLocked(windChimePortal, false);
        SetPortalLocked(crystalCavesPortal, false);
        SetPortalLocked(starFishPondPortal, false);
        SetPortalLocked(tallGrassPortal, false);

        // Teapot starts locked - updated every frame below
        SetPortalLocked(grandTeapotPortal, true);
    }

    private void Update()
    {
        RefreshTeapotLock();
    }

    /// <summary>
    /// Checks GameManager's win condition to decide whether to unlock the Teapot path.
    /// </summary>
    private void RefreshTeapotLock()
    {
        if (gameManager == null) return;

        bool allIngredients = gameManager.checkWinCondition();
        SetPortalLocked(grandTeapotPortal, !allIngredients);

        if (teapotLockedUI != null)
            teapotLockedUI.SetActive(!allIngredients);
    }

    private void SetPortalLocked(HubPathPortal portal, bool locked)
    {
        if (portal != null)
            portal.SetLocked(locked);
    }

    /// <summary>
    /// Called by HubPathPortal when the player confirms they want to enter a path.
    /// </summary>
    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return;
        SceneTransitionManager.Instance.LoadScene(sceneName);
    }
}
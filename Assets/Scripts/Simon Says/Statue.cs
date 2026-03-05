using UnityEngine;
using UnityEngine.InputSystem;

public class Statue : MonoBehaviour
{
   InputAction jumpAction;
   SimonSaysPuzzle simonSaysPuzzleScript;
   bool playerInZone;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        simonSaysPuzzleScript = transform.parent.gameObject.GetComponent<SimonSaysPuzzle>();
        jumpAction = InputSystem.actions.FindAction("Jump");
    }

    // Update is called once per frame
    void Update()
    {
    // Check if the player is standing in the zone AND pressed jump this frame
        if (playerInZone && jumpAction.WasPerformedThisFrame())
        {
            simonSaysPuzzleScript.PlayerClicked(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = false;
        }
    }
}

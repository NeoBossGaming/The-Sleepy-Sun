using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControllerTallGrass : MonoBehaviour
{
   public bool isHidden = false;
    InputAction moveAction;
    [SerializeField] private Rigidbody2D rb;
    Vector2 moveValue;
    [SerializeField] private float speed = 200f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        moveAction = InputSystem.actions.FindAction("Move");
    }

    // Update is called once per frame
    void Update()
    {
        moveValue = moveAction.ReadValue<Vector2>();
        Debug.Log(moveValue);
    }

    public void Dead()
    {
        
    }

    private void FixedUpdate() {
        
        rb.linearVelocity = moveValue * Time.deltaTime * speed;
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Grass")) {
            isHidden = true;
            // Optional: Lower the player's opacity to show they are hidden
            GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.5f); 
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        if (other.CompareTag("Grass")) {
            isHidden = false;
            GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1f);
        }
    }

}

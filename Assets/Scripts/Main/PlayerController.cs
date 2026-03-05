using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    InputAction moveAction;
    [SerializeField] private Rigidbody2D rb;
    Vector2 moveValue;
    [SerializeField] private float speed = 200f;
    [SerializeField] Animator animator;
    
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

        // 1. Check Vertical Up
        if (moveValue.y >= 0.5f)
        {
            SetAnimation("Walking Up");
        }
        // 2. Check Vertical Down
        else if (moveValue.y <= -0.5f)
        {
            SetAnimation("Walking Down");
        }
        // 3. Check Horizontal Right
        else if (moveValue.x >= 0.5f)
        {
            SetAnimation("Walking Right");
        }
        // 4. Check Horizontal Left
        else if (moveValue.x <= -0.5f)
        {
            SetAnimation("Walking Left");
        }
        // 5. Idle (No movement)
        else
        {
            SetAnimation("Idle");
        }
    }

    // This helper function saves you from typing out 5 lines for every direction
    void SetAnimation(string activeName)
    {
        animator.SetBool("Walking Up", activeName == "Walking Up");
        animator.SetBool("Walking Down", activeName == "Walking Down");
        animator.SetBool("Walking Left", activeName == "Walking Left");
        animator.SetBool("Walking Right", activeName == "Walking Right");
        animator.SetBool("Idle", activeName == "Idle");
    }

    private void FixedUpdate() {
        rb.linearVelocity = moveValue * Time.deltaTime * speed;
    }


}

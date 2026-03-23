using UnityEngine;

/// <summary>
/// Base movement script for all scene-specific player controllers.
/// 
/// HOW TO EXTEND:
/// - Override HandleInput() to add new inputs (e.g., jump, select, dash).
/// - Override ApplyMovement() to change how physics is applied.
/// - Call SetCanMove(false) from other scripts to freeze the player.
/// 
/// Requires PlayerInput and Rigidbody2D on the same GameObject.
/// </summary>
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] protected float speed = 200f;
    [SerializeField] protected bool canMove = true;

    protected PlayerInput playerInput;
    protected Rigidbody2D rb;
    protected Vector2 moveValue;

    protected virtual void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void Update()
    {
        ReadInput();
        HandleInput();
    }

    protected virtual void FixedUpdate()
    {
        ApplyMovement();
    }

    /// <summary>
    /// Reads the move vector from PlayerInput each frame.
    /// Stored in moveValue for use in ApplyMovement and subclasses.
    /// </summary>
    protected virtual void ReadInput()
    {
        moveValue = playerInput.obtainMoveInputActions().move;
    }

    /// <summary>
    /// Override this in subclasses to respond to button inputs (dash, jump, interact, etc.).
    /// Called every Update() after ReadInput(). 
    /// Button presses (WasPressedThisFrame) MUST be checked here, not in FixedUpdate.
    /// </summary>
    protected virtual void HandleInput() { }

    /// <summary>
    /// Applies velocity to the Rigidbody2D. Called every FixedUpdate.
    /// Override to change movement style (e.g., force-based, locked-axis).
    /// </summary>
    protected virtual void ApplyMovement()
    {
        if (!canMove)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = moveValue * speed * Time.fixedDeltaTime;
    }

    // --- Public API ---

    public void SetCanMove(bool value) => canMove = value;

    public Vector2 GetMoveValue() => moveValue;
}
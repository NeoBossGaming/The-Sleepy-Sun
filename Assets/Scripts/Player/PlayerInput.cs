using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    // Action Reference
    InputAction moveInput;
    InputAction lookInput;
    InputAction dashInput;
    InputAction interactInput;
    InputAction previousInput;
    InputAction nextInput;
    InputAction sprintInput;

    // Action Value
    public struct inputValues
    {
        public Vector2 move;
        public Vector2 look;
        public bool dash;
        public bool interact;
        public bool previous;
        public bool next;
        public  bool sprint;
    }

    private inputValues currentInputValues;

    void Start()
    {
        // Initialize all actions
        moveInput = InputSystem.actions.FindAction("Move");
        lookInput = InputSystem.actions.FindAction("Look");
        dashInput = InputSystem.actions.FindAction("Dash");
        interactInput = InputSystem.actions.FindAction("Interact");
        previousInput = InputSystem.actions.FindAction("Previous");
        nextInput = InputSystem.actions.FindAction("Next");
        sprintInput = InputSystem.actions.FindAction("Sprint");

        // Enable all inputs
        moveInput.Enable();
        lookInput.Enable();
        dashInput.Enable();
        interactInput.Enable();
        previousInput.Enable();
        nextInput.Enable();
        sprintInput.Enable();
    }
    
    void Update()
    {
        // Obtain Vector 2 Input Values
        currentInputValues.move = moveInput.ReadValue<Vector2>();
        currentInputValues.look = lookInput.ReadValue<Vector2>();

        // Obtain Bool Buttons Input Values
        currentInputValues.dash = dashInput.WasPressedThisFrame();
        currentInputValues.interact = interactInput.WasPressedThisFrame();
        currentInputValues.previous = previousInput.WasPressedThisFrame();
        currentInputValues.next = nextInput.WasPressedThisFrame();

        // Obtain Continous Button Input Values
        currentInputValues.sprint = sprintInput.IsPressed();
    }

    /// <summary>
    /// Retrieves all input values from the struct.
    /// Stores all of the input values with data type bool,
    /// except for input move and look (Vector2).
    /// </summary>
    /// <returns>Struct containing all input values.</returns>
    public inputValues obtainMoveInputActions()
    {
        return currentInputValues;
    } 

}

using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class RhythmPlayerController : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private float scoreSurvived;
    InputAction moveAction;
    InputAction jumpAction;
    [SerializeField] private Rigidbody2D rb;
    Vector2 moveValue;
    [SerializeField] private float speed = 200f;

    [SerializeField] private LayerMask groundLayer;
    private bool ded;
    [SerializeField] private TopDownJump jumpScript;
    [SerializeField] public bool isStationary = true;
    [SerializeField] public bool ableToLandMove = true;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        jumpScript = GetComponent<TopDownJump>();
        rb = GetComponent<Rigidbody2D>();
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        ded = false;
    }

    public Vector2 getMoveValue()
    {
        return moveAction.ReadValue<Vector2>();
    }
    public bool getJumpValue()
    {
        return jumpAction.IsPressed();
    }

    private void Update() {
        if (jumpScript.currentLeafAnchor != null)
        {
            if (jumpScript.currentLeafAnchor.thisLeaf.isSafe == false)
            {
                Dead();
            }
        }
        scoreSurvived += Time.deltaTime;
        scoreText.text = !ded ? $"Score: {Mathf.Round(scoreSurvived)}" : "You Ded";
    }

    private void Dead ()
    {
        Debug.Log("You ded");
        jumpScript.isBusy = true;
        ded = true;
        transform.SetParent(null, true);
    }

    private void FixedUpdate() {
        if (isStationary)
        {
            if (ableToLandMove)
            {
                landMove();
            } else
            {
                rb.linearVelocity = new Vector2(0, 0);
            }
        }
    } 

    private void landMove()
    {
        float horizontalMovement = getMoveValue()[0];
        rb.linearVelocity = new Vector2(horizontalMovement * speed * Time.deltaTime, rb.linearVelocity.y);
    }
}

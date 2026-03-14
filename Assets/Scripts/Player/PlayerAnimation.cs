using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerInput))]
public class PlayerAnimation : MonoBehaviour
{
    private PlayerInput playerInputScript;
    private Animator playerAnimator;

    private bool runAnimation = true;

    [Header("Animation Options")]
    [SerializeField] private bool upAnimation, downAnimation, leftAnimation, rightAnimation, idleAnimation;

    [SerializeField] private float updateAnimationStateFrequency = 0.5f;

    void Start() {
        StartCoroutine(UpdateAnimationState());
    }

    /// <summary>
    /// Checks input values for move value, and then calls SetAnimation to set
    /// player animation to the corresponding input.
    /// </summary>
    IEnumerator UpdateAnimationState()
    {
        while (runAnimation) 
        {
            Vector2 moveValue = playerInputScript.obtainMoveInputActions().move;
            // 1. Check Vertical Up
            if (moveValue.y >= 0.5f && upAnimation)
            {
                SetAnimationState("Walking Up");
            }
            // 2. Check Vertical Down
            else if (moveValue.y <= -0.5f && downAnimation)
            {
                SetAnimationState("Walking Down");
            }
            // 3. Check Horizontal Right
            else if (moveValue.x >= 0.5f && rightAnimation)
            {
                SetAnimationState("Walking Right");
            }
            // 4. Check Horizontal Left
            else if (moveValue.x <= -0.5f && leftAnimation)
            {
                SetAnimationState("Walking Left");
            }
            // 5. Idle (No movement)
            else if (idleAnimation)
            {
                SetAnimationState("Idle");
            }
            yield return new WaitForSeconds(updateAnimationStateFrequency);
        }
    }

    /// <summary>
    /// Collects current input state, and plays the correct animation correspondingly.
    /// </summary>
    /// <param name="activeName">The current walking state</param>
    void SetAnimationState(string activeName)
    {
        playerAnimator.SetBool("Walking Up", activeName == "Walking Up");
        playerAnimator.SetBool("Walking Down", activeName == "Walking Down");
        playerAnimator.SetBool("Walking Left", activeName == "Walking Left");
        playerAnimator.SetBool("Walking Right", activeName == "Walking Right");
        playerAnimator.SetBool("Idle", activeName == "Idle");
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    private Vector2 movement;
    public bool canMove = true;

    [Header("Components")]
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    // Hashes for safe checking
    private int walkUpHash;
    private int walkDownHash;
    private int diagUpLeftHash;
    private int diagDownRightHash;

    private bool hasWalkUp;
    private bool hasWalkDown;
    private bool hasDiagUpLeft;
    private bool hasDiagDownRight;

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        walkUpHash = Animator.StringToHash("WalkUp");
        walkDownHash = Animator.StringToHash("WalkDown");
        diagUpLeftHash = Animator.StringToHash("DiagUpLeft");
        diagDownRightHash = Animator.StringToHash("DiagDownRight");

        hasWalkUp = animator.HasState(0, walkUpHash);
        hasWalkDown = animator.HasState(0, walkDownHash);
        hasDiagUpLeft = animator.HasState(0, diagUpLeftHash);
        hasDiagDownRight = animator.HasState(0, diagDownRightHash);
    }

    void Update()
    {
        if (canMove)
        {
            movement.x = Input.GetAxisRaw("Horizontal");
            movement.y = Input.GetAxisRaw("Vertical");

            float moveMagnitude = Mathf.Abs(movement.x) + Mathf.Abs(movement.y);
            animator.SetFloat("Speed", moveMagnitude);

            
            if (movement.x > 0.1f && movement.y > 0.1f && hasDiagDownRight)
            {
                // Down-Right
                spriteRenderer.flipX = true;
                animator.Play(diagDownRightHash);
                return;
            }

            if (movement.x < -0.1f && movement.y < -0.1f && hasDiagUpLeft)
            {
                // Up-Left
                spriteRenderer.flipX = true;
                animator.Play(diagUpLeftHash);
                return;
            }

            if (movement.x < -0.1f && movement.y > 0.1f && hasDiagDownRight)
            {
                // Down-Left (flip Down-Right)
                spriteRenderer.flipX = false;
                animator.Play(diagDownRightHash);
                return;
            }

            if (movement.x > 0.1f && movement.y < -0.1f && hasDiagUpLeft)
            {
                // Up-Right (flip Up-Left)
                spriteRenderer.flipX = false;
                animator.Play(diagUpLeftHash);
                return;
            }

            // vertical only
            if (movement.y > 0.1f && hasWalkUp)
            {
                animator.Play(walkUpHash);
                return;
            }
            if (movement.y < -0.1f && hasWalkDown)
            {
                animator.Play(walkDownHash);
                return;
            }

            // horizontal only
            if (Mathf.Abs(movement.x) > 0.1f)
            {
                spriteRenderer.flipX = movement.x < 0;
                animator.Play("Run");
                return;
            }

            // idle
            animator.Play("Idle");
        }
        else
        {
            movement = Vector2.zero;
            animator.SetFloat("Speed", 0);
            animator.Play("Idle");
        }
    }

    void FixedUpdate()
    {
        if (canMove)
        {
            transform.Translate(movement * moveSpeed * Time.fixedDeltaTime);
        }
    }
   
}

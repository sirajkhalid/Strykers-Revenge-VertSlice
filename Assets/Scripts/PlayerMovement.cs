using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f; // Speed of the character
    private Vector2 movement;
    public bool canMove = true; // Flag to enable/disable movement

    [Header("Components")]
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (canMove)
        {
            // Get input from arrow keys or WASD
            movement.x = Input.GetAxisRaw("Horizontal");
            movement.y = Input.GetAxisRaw("Vertical");

            // Update the Speed parameter to switch between idle/run
            animator.SetFloat("Speed", Mathf.Abs(movement.x) + Mathf.Abs(movement.y));

            // Flip sprite horizontally when moving left/right
            if (movement.x != 0)
                spriteRenderer.flipX = movement.x < 0;
        }
        else
        {
            // Stop movement in battle mode
            movement = Vector2.zero;
            animator.SetFloat("Speed", 0);
        }
    }

    void FixedUpdate()
    {
        if (canMove)
        {
            // Move the character
            transform.Translate(movement * moveSpeed * Time.fixedDeltaTime);
        }
    }
}

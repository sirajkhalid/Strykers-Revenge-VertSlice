using UnityEngine;

public class RatMove : MonoBehaviour
{
    private Animator animator;
    private SpriteRenderer sprite;
    public float speed = 2f;
    public float detectDistance = 0.4f;
    public float turnTime = 0.5f;

    private Vector2 direction;
    private Rigidbody2D rb;
    private float turnCooldown;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        FiDirection();
        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
    }

    void FixedUpdate()
    {
        turnCooldown -= Time.fixedDeltaTime;

        RaycastHit2D hit = Physics2D.Raycast(rb.position, direction, detectDistance);

        if (hit.collider != null && turnCooldown <= 0f)
        {
            FiDirection();
            turnCooldown = turnTime;
        }

        rb.linearVelocity = direction * speed;

        animator.SetFloat("speed", rb.linearVelocity.sqrMagnitude);

        if (direction.y != 0)
            sprite.flipY = direction.y < 0;
    }

    void FiDirection()
    {
        int dir = Random.Range(0, 4);

        direction = dir switch
        {
            0 => Vector2.up,
            1 => Vector2.down,
            2 => Vector2.left,
            _ => Vector2.right
        };
    }
}

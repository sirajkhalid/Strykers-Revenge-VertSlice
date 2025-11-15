using UnityEngine;

public class SimplePatrol : MonoBehaviour
{
    [Header("Points")]
    public Vector2 pointA;                 // auto-filled at Start
    public Vector2 pointB = new Vector2(2f, 0f);

    [Header("Settings")]
    public float moveSpeed = 2f;
    public float waitTime = 1f;

    [Header("References")]
    public Animator anim;
    public SpriteRenderer spriteRenderer;

    private Vector2 currentTarget;
    private bool waiting = false;

    void Start()
    {
        // Automatically set point A to the starting position
        pointA = transform.position;

        // Move first toward point B
        currentTarget = pointB;
    }

    void Update()
    {
        Patrol();
    }

    void Patrol()
    {
        if (waiting) return;

      
        transform.position = Vector2.MoveTowards(transform.position, currentTarget, moveSpeed * Time.deltaTime);

        bool isMoving = Vector2.Distance(transform.position, currentTarget) > 0.05f;
        anim.SetBool("IsWalking", isMoving);

        // Flip sprite only while moving
        if (isMoving)
        {
            spriteRenderer.flipX = currentTarget.x > transform.position.x;

        }

        // Reached target
        if (!isMoving)
        {
            StartCoroutine(SwitchPoint());
        }
    }

    System.Collections.IEnumerator SwitchPoint()
    {
        waiting = true;

        // Play idle
        anim.SetBool("IsWalking", false);

        // Wait at point
        yield return new WaitForSeconds(waitTime);

        // Switch target
        if (currentTarget == pointA)
            currentTarget = pointB;
        else
            currentTarget = pointA;

        waiting = false;
    }
}

using System.Collections;
using UnityEngine;

public enum EnemyMovementType
{
    Idle,
    BackAndForth,
    RandomWander,
    CirclePath,
    ChasePlayer
}

public class EnemyMovementController : MonoBehaviour
{
    [Header("Movement Settings")]
    public EnemyMovementType movementType = EnemyMovementType.Idle;
    public float moveSpeed = 2f;

    [Header("Back and Forth")]
    public float patrolDistance = 3f;
    public Vector2 patrolDirection = Vector2.right;

    [Header("Random Wander")]
    public float wanderRadius = 4f;
    public float wanderPauseTime = 0.5f;

    [Header("Circle Path")]
    public float circleRadius = 2f;
    public float circleAngularSpeed = 90f;
    public bool circleRandomizeDirection = true;

    [Header("Chase Behavior")]
    public float chaseSpeedMultiplier = 1.5f;

    [Header("Chase Stop Conditions")]
    public float chaseMaxDistance = 8f;     
    public float chaseMaxTime = 4f;         
    private float chaseTimer = 0f;

    // Auto-detected follower flag
    private bool isFollower = false;

    [HideInInspector] public bool canMove = true;
    [HideInInspector] public bool externalControl = false;

    private Vector3 externalTarget;
    private Vector3 originPos;

    // Back/forth
    private Vector3 pointA;
    private Vector3 pointB;
    private bool goingToB = true;

    // Wander
    private Vector3 wanderTarget;
    private float wanderTimer;
    private bool hasWanderTarget;

    // Circle
    private float circleAngle;
    private int circleDirection = 1;

    private Animator animator;
    private PlayerPartyController partyController;
    private Transform player;

    public bool isInBattle = false;
    private Vector3 lastPosition;
    private bool battleMoving = false;
    private bool hasFollowTarget = false;

    private float stuckTimer = 0f;
    private Vector3 lastMoveCheckPos;
    private float lastDistanceToPlayer = -1f;


    void Awake()
    {
        originPos = transform.position;
        animator = GetComponentInChildren<Animator>();

        // Followers = children of another enemy
        isFollower = (
            transform.parent != null &&
            transform.parent.GetComponent<EnemyMovementController>() != null
        );

        partyController = FindFirstObjectByType<PlayerPartyController>();
        if (partyController != null && partyController.activeMember != null)
            player = partyController.activeMember.transform;
    }


    void Start()
    {
        SetupMovementPattern();
        lastPosition = transform.position;

    }

    void Update()
    {
        if (partyController != null && partyController.activeMember != null)
            player = partyController.activeMember.transform;



        if (!canMove)
        {
            UpdateAnimation(false);
            return;
        }

        bool isMoving = false;

        
        if (isFollower)
        {
            if (hasFollowTarget)
            {
                isMoving = MoveTowards(externalTarget, moveSpeed);
                hasFollowTarget = false; 
            }
            else
            {
                isMoving = false;
            }
        }

        else
        {
            // LEADER
            if (externalControl)
            {
                isMoving = MoveTowards(externalTarget, moveSpeed);
            }
            else
            {
                switch (movementType)
                {
                    case EnemyMovementType.Idle:
                        isMoving = false;
                        break;
                    case EnemyMovementType.BackAndForth:
                        isMoving = UpdateBackAndForth();
                        break;
                    case EnemyMovementType.RandomWander:
                        isMoving = UpdateRandomWander();
                        break;
                    case EnemyMovementType.CirclePath:
                        isMoving = UpdateCircle();
                        break;
                    case EnemyMovementType.ChasePlayer:
                        isMoving = UpdateChase();
                        break;
                }
            }
        }
        if (isInBattle)
        {
            // detect if the AI is moving the enemy
            float moved = Vector3.Distance(transform.position, lastPosition);
            battleMoving = moved > 0.001f;

            UpdateAnimation(battleMoving);

            lastPosition = transform.position;
            return;
        }

        UpdateAnimation(isMoving);
    }


    // Setup

    private void SetupMovementPattern()
    {
        // Back & Forth
        if (movementType == EnemyMovementType.BackAndForth)
        {
            Vector2 dir = patrolDirection.sqrMagnitude < 0.01f ? Vector2.right : patrolDirection.normalized;
            pointA = originPos - (Vector3)(dir * patrolDistance * 0.5f);
            pointB = originPos + (Vector3)(dir * patrolDistance * 0.5f);
        }

        // Circle
        if (movementType == EnemyMovementType.CirclePath)
        {
            circleAngle = Random.Range(0f, 360f);
            circleDirection = circleRandomizeDirection && Random.value < 0.5f ? -1 : 1;
        }
    }


    // Movement Modes

    private bool UpdateBackAndForth()
    {
        Vector3 target = goingToB ? pointB : pointA;
        bool moving = MoveTowards(target, moveSpeed);

        if (Vector3.Distance(transform.position, target) < 0.1f)
            goingToB = !goingToB;

        return moving;
    }

    private bool UpdateRandomWander()
    {
        if (!hasWanderTarget)
        {
            ChooseNewWanderTarget();
            return false;
        }

        if (wanderTimer > 0f)
        {
            wanderTimer -= Time.deltaTime;
            return false;
        }

        bool moving = MoveTowards(wanderTarget, moveSpeed);

        if (Vector3.Distance(transform.position, wanderTarget) < 0.1f)
        {
            hasWanderTarget = false;
            wanderTimer = wanderPauseTime;
        }

        return moving;
    }

    private void ChooseNewWanderTarget()
    {
        Vector2 rand = Random.insideUnitCircle * wanderRadius;
        wanderTarget = originPos + new Vector3(rand.x, rand.y, 0f);
        hasWanderTarget = true;
    }

    private bool UpdateCircle()
    {
        circleAngle += circleAngularSpeed * circleDirection * Time.deltaTime;

        float rad = circleAngle * Mathf.Deg2Rad;
        Vector3 target = originPos + new Vector3(Mathf.Cos(rad) * circleRadius,
                                                Mathf.Sin(rad) * circleRadius,
                                                0f);

        return MoveTowards(target, moveSpeed);
    }

    private bool UpdateChase()
    {
        if (player == null)
            return false;

        float distance = Vector3.Distance(transform.position, player.position);

        // Count chase time
        chaseTimer += Time.deltaTime;

        // Give up conditions
        if (distance > chaseMaxDistance || chaseTimer > chaseMaxTime)
        {
            // Reset chase
            movementType = EnemyMovementType.Idle;
            externalControl = false;
            chaseTimer = 0f;

            // Return to origin
            StartCoroutine(ReturnToOrigin());
            return false;
        }

        float speed = moveSpeed * chaseSpeedMultiplier;
        return MoveTowards(player.position, speed);
    }

    private IEnumerator ReturnToOrigin()
    {
        float speed = moveSpeed;
        Vector3 target = originPos;

        while (Vector3.Distance(transform.position, target) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                target,
                speed * Time.deltaTime
            );

            UpdateAnimation(true);
            yield return null;
        }

        UpdateAnimation(false);
    }


    // Helpers

    public bool MoveTowards(Vector3 target, float speed)
    {
        Vector3 pos = transform.position;
        Vector3 dir = target - pos;

        // Prevent sudden teleport if target is too far away
        if (dir.magnitude > 2f)
            dir = dir.normalized * 2f;

        if (dir.sqrMagnitude < 0.0001f)
            return false;

        // Flip sprite based on horizontal direction
        if (dir.x > 0.01f)
        {
            Vector3 scale = transform.localScale;
            scale.x = -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
        else if (dir.x < -0.01f)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

 
        // MOVE TOWARD TARGET
 
        transform.position = pos + dir.normalized * speed * Time.deltaTime;



        // SMART STUCK DETECTION

        if (movementType == EnemyMovementType.ChasePlayer && player != null)
        {
            float currentDist = Vector3.Distance(transform.position, player.position);

            // initialize on start
            if (lastDistanceToPlayer < 0f)
                lastDistanceToPlayer = currentDist;

            // check every frame 
            if (Mathf.Abs(currentDist - lastDistanceToPlayer) < 0.015f)
            {
                // Distance to player is NOT changing → likely stuck
                stuckTimer += Time.deltaTime;
            }
            else
            {
                // moving properly → reset stuck logic
                stuckTimer = 0f;
            }

            // After 2 sec of not getting closer → assume stuck
            if (stuckTimer >= 2f)
            {
                Vector3 verticalDir =
                    (player.position.y > transform.position.y)
                    ? Vector3.up
                    : Vector3.down;

                transform.position += verticalDir * (speed * Time.deltaTime);

                stuckTimer = 0f;
            }

            lastDistanceToPlayer = currentDist;
        }


        return true;
    }

    private void UpdateAnimation(bool isMoving)
    {
        if (animator == null) return;
        animator.SetBool("Walk", isMoving);
    }


    // External control 

    public void SetExternalTarget(Vector3 worldPosition)
    {
        externalControl = true;
        externalTarget = worldPosition;
    }

    public void ClearExternalControl()
    {
        externalControl = false;
    }

    public void ForceChasePlayer()
    {
        movementType = EnemyMovementType.ChasePlayer;
        externalControl = false;
    }

    public void OnBattleStarted()
    {
        isInBattle = true;

        externalControl = false;
        movementType = EnemyMovementType.Idle;

    }

    public void ReceiveFollowTarget(Vector3 target)
    {
        externalTarget = target;
        hasFollowTarget = true;
        externalControl = true;
    }


}

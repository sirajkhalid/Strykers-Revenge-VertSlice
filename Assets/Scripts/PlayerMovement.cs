using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Speeds")]
    public float smoothing = 15f;
    public float sprintBonus = 3f;

    private Vector2 rawInput;
    private Vector2 smoothVelocity;

    private float baseMoveSpeed = 5f; 

    [Header("Footstep Audio")]
    public AudioSource audioSource;
    public AudioClip walkFootstepSFX;
    public AudioClip sprintFootstepSFX;
    private float footstepTimer;

    [Header("Components")]
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private CharacterStats stats;

    // Animation 
    private int walkUpHash;
    private int walkDownHash;
    private int runHash;
    private int idleHash;
    private int sprintHash;

    
    private int diagUpLeftHash;
    private int diagDownRightHash;

    private bool hasWalkUp, hasWalkDown, hasRun, hasIdle, hasSprint;
    private bool hasDiagUpLeft, hasDiagDownRight;

    private bool wasSprinting = false;

    public bool canMove = true;

    void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        stats = GetComponent<CharacterStats>();

        // animation state hashes
        walkUpHash = Animator.StringToHash("WalkUp");
        walkDownHash = Animator.StringToHash("WalkDown");
        runHash = Animator.StringToHash("Run");
        idleHash = Animator.StringToHash("Idle");
        sprintHash = Animator.StringToHash("Sprint");

        diagUpLeftHash = Animator.StringToHash("DiagUpLeft");
        diagDownRightHash = Animator.StringToHash("DiagDownRight");

        hasWalkUp = animator.HasState(0, walkUpHash);
        hasWalkDown = animator.HasState(0, walkDownHash);
        hasRun = animator.HasState(0, runHash);
        hasIdle = animator.HasState(0, idleHash);
        hasSprint = animator.HasState(0, sprintHash);

        hasDiagUpLeft = animator.HasState(0, diagUpLeftHash);
        hasDiagDownRight = animator.HasState(0, diagDownRightHash);

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        
        if (stats != null && stats.isCasting)
        {
            animator.SetFloat("Speed", 0);
            HandleFootsteps(false);
            return;
        }

        if (!canMove)
        {
            rawInput = Vector2.zero;
            animator.SetFloat("Speed", 0f);
            animator.Play(idleHash);
            HandleFootsteps(false);
            return;
        }

        // ----------------------------
        // MOVEMENT INPUT
        // ----------------------------
        rawInput.x = Input.GetAxisRaw("Horizontal");
        rawInput.y = Input.GetAxisRaw("Vertical");
        rawInput = rawInput.normalized;

        
        smoothVelocity = Vector2.Lerp(smoothVelocity, rawInput, Time.deltaTime * smoothing);

        float moveMagnitude = Mathf.Abs(rawInput.x) + Mathf.Abs(rawInput.y);
        animator.SetFloat("Speed", moveMagnitude);

        float absX = Mathf.Abs(rawInput.x);
        float absY = Mathf.Abs(rawInput.y);

        // ----------------------------
        // SPRINT STATE and CAMERA SHAKE
        // ----------------------------
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);

        // Trigger camera pulse ONLY when sprint starts
        if (isSprinting && !wasSprinting)
        {
            var party = FindFirstObjectByType<PlayerPartyController>();
            if (party != null)
                party.TriggerSprintCameraShake();
        }
        wasSprinting = isSprinting;

        // --------------------------------
        // OPTIONAL DIAGONAL ANIMATIONS 
        // --------------------------------
        /*
        if (absX > 0.1f && absY > 0.1f)
        {
            if (rawInput.x > 0 && rawInput.y > 0 && hasDiagDownRight)
            {
                spriteRenderer.flipX = true;
                animator.Play(diagDownRightHash);
                return;
            }
            if (rawInput.x < 0 && rawInput.y < 0 && hasDiagUpLeft)
            {
                spriteRenderer.flipX = true;
                animator.Play(diagUpLeftHash);
                return;
            }
            if (rawInput.x < 0 && rawInput.y > 0 && hasDiagDownRight)
            {
                spriteRenderer.flipX = false;
                animator.Play(diagDownRightHash);
                return;
            }
            if (rawInput.x > 0 && rawInput.y < 0 && hasDiagUpLeft)
            {
                spriteRenderer.flipX = false;
                animator.Play(diagUpLeftHash);
                return;
            }
        }
        */

        // ----------------------------
        // ANIMATION CHOICES
        // ----------------------------
        // Up / Down
        if (rawInput.y > 0.1f && hasWalkUp)
        {
            animator.Play(walkUpHash);
        }
        else if (rawInput.y < -0.1f && hasWalkDown)
        {
            animator.Play(walkDownHash);
        }
        // Left / Right
        else if (absX > 0.1f)
        {
            spriteRenderer.flipX = rawInput.x < 0;

            if (isSprinting && hasSprint)
                animator.Play(sprintHash);
            else
                animator.Play(runHash);
        }
        else
        {
            animator.Play(idleHash);
        }

        // ----------------------------
        // FOOTSTEP SFX
        // ----------------------------
        HandleFootsteps(isSprinting);
    }


    void FixedUpdate()
    {
        if (!canMove) return;

        float speed = CalculateMovementSpeed();
        Vector2 move = smoothVelocity * speed * Time.fixedDeltaTime;
        transform.Translate(move);
    }

    // ------------------------------------------------------
    // Calculate movement speed (CharacterStats + bonus)
    // ------------------------------------------------------
    float CalculateMovementSpeed()
    {
        float move = baseMoveSpeed;

        if (stats != null)
            move = stats.maxMovement / 6f;

        move += 3f; // your buff
        if (Input.GetKey(KeyCode.LeftShift))
            move += sprintBonus;

        return move;
    }

    // ------------------------------------------------------
    // FOOTSTEP AUDIO SYSTEM
    // ------------------------------------------------------
    void HandleFootsteps(bool sprinting)
    {
        
        if (rawInput.magnitude < 0.1f || !canMove)
        {
            if (audioSource.isPlaying)
                audioSource.Stop();
            return;
        }

        AudioClip desiredClip = sprinting ? sprintFootstepSFX : walkFootstepSFX;

        
        if (audioSource.clip != desiredClip)
        {
            audioSource.clip = desiredClip;
            audioSource.loop = true;
            audioSource.Play();
        }

        
        if (!audioSource.isPlaying)
        {
            audioSource.clip = desiredClip;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

}

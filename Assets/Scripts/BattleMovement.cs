using UnityEngine;
using TMPro;

public class BattleMovement : MonoBehaviour
{
    [Header("References")]
    public CharacterStats characterStats;
    public PlayerMovement playerMovement;
    public BattleStateManager battleManager;
    public TMP_Text movementText;           // TMP in BattleUI -> MovementPanel

    [Header("Movement Settings")]
    public float baseRange = 6f;            // base meters everyone can move
    public float dexMultiplier = 0.5f;      // each Dex adds 0.5m
    public float currentMovement;
    public float maxMovement;

    private Vector3 lastPosition;
    private bool wasBattleActive;

    void Start()
    {
        if (characterStats == null)
            characterStats = GetComponent<CharacterStats>();

        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();

        if (battleManager == null)
            battleManager = FindFirstObjectByType<BattleStateManager>();

        lastPosition = transform.position;
    }

    void Update()
    {
        // Detect battle start / end automatically
        if (battleManager != null)
        {
            if (battleManager.isBattleActive && !wasBattleActive)
                BeginBattleMovement();
            else if (!battleManager.isBattleActive && wasBattleActive)
                EndBattleMovement();

            wasBattleActive = battleManager.isBattleActive;
        }

        if (!battleManager || !battleManager.isBattleActive)
            return;

        TrackMovement();
        UpdateUI();
    }

    void BeginBattleMovement()
    {
        // calculate range from Dexterity
        maxMovement = baseRange + (characterStats.dexterity * dexMultiplier);
        currentMovement = maxMovement;
        lastPosition = transform.position;
        playerMovement.canMove = true;
    }

    void EndBattleMovement()
    {
        playerMovement.canMove = true;
        if (movementText != null)
            movementText.text = "";
    }

    void TrackMovement()
    {
        float distanceThisFrame = Vector3.Distance(transform.position, lastPosition);

        if (distanceThisFrame > 0f)
        {
            currentMovement -= distanceThisFrame;
            if (currentMovement <= 0f)
            {
                currentMovement = 0f;
                playerMovement.canMove = false;
            }
            lastPosition = transform.position;
        }
    }

    void UpdateUI()
    {
        if (movementText == null) return;
        movementText.text = $"{currentMovement:F2}m / {maxMovement:F2}m";
    }
}

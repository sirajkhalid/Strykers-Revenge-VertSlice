using UnityEngine;
using TMPro;

public class BattleMovement : MonoBehaviour
{
    [Header("References")]
    public CharacterStats characterStats;
    public PlayerMovement playerMovement;
    public BattleStateManager battleManager;
    public TMP_Text movementText; // TMP in BattleUI -> MovementPanel

    [Header("Movement Settings")]
    public float baseRange = 6f;
    public float dexMultiplier = 0.5f;
    public float currentMovement;
    public float maxMovement;

    private Vector3 lastPosition;
    private bool wasBattleActive;
    private bool wasPlayerTurn;
  
    private TurnManager turnManager;

    void Start()
    {
        if (characterStats == null)
            characterStats = GetComponent<CharacterStats>();

        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();

        if (battleManager == null)
            battleManager = FindFirstObjectByType<BattleStateManager>();

        if (movementText == null)
        {
            // Try by object name
            var found = GameObject.Find("MovementNum");
            if (found != null)
                movementText = found.GetComponent<TMP_Text>();

            // Fallback
            if (movementText == null)
                movementText = FindFirstObjectByType<TMP_Text>();
        }

        turnManager = FindFirstObjectByType<TurnManager>();

        lastPosition = transform.position;
    }



    void Update()
    {
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

        // Wait for TurnManager
        if (turnManager == null)
            return;

        // Detect turn changes
        if (turnManager.isPlayerTurn && !wasPlayerTurn)
        {
            // Player's turn just started
            ResetMovementForNewTurn();
        }

        wasPlayerTurn = turnManager.isPlayerTurn;

        // Disable player movement if it's not their turn
        if (!turnManager.isPlayerTurn)
        {
            playerMovement.canMove = false;
            return;
        }

        TrackMovement();
        UpdateUI();
    }

    void BeginBattleMovement()
    {
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

    void ResetMovementForNewTurn()
    {
        currentMovement = maxMovement;
        playerMovement.canMove = true;
        lastPosition = transform.position;
        UpdateUI();
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

using UnityEngine;
using TMPro;

public class BattleMovement : MonoBehaviour
{
    [Header("References")]
    public CharacterStats characterStats;
    public PlayerMovement playerMovement;
    public BattleStateManager battleManager;
    public TMP_Text movementText;

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
            var found = GameObject.Find("MovementNum");
            if (found != null)
                movementText = found.GetComponent<TMP_Text>();
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

        if (turnManager == null)
            return;

        // Detect start of player turn
        if (turnManager.isPlayerTurn && !wasPlayerTurn)
            ResetMovementForNewTurn();

        wasPlayerTurn = turnManager.isPlayerTurn;

        // Disable movement if not player turn
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
        // Use CharacterStats-calculated movement
        characterStats.ResetMovement();
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
        characterStats.ResetMovement();
        lastPosition = transform.position;
        playerMovement.canMove = true;
        UpdateUI();
    }

    void TrackMovement()
    {
        float moved = Vector3.Distance(transform.position, lastPosition);

        if (moved > 0f)
        {
            characterStats.currentMovement -= moved;

            if (characterStats.currentMovement <= 0f)
            {
                characterStats.currentMovement = 0f;
                playerMovement.canMove = false;
            }

            lastPosition = transform.position;
        }

        // If an ability restored movement, unlock movement again
        if (characterStats.currentMovement > 0f && turnManager.isPlayerTurn)
        {
            playerMovement.canMove = true;
        }
    }


    void UpdateUI()
    {
        if (movementText == null) return;
        movementText.text =
            $"{characterStats.currentMovement:F2}m / {characterStats.maxMovement:F2}m";
    }
}

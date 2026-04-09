using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

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

        SceneManager.sceneLoaded += this.OnLoadCallBack;
    }

    void OnLoadCallBack(Scene scene, LoadSceneMode sceneMode) // potiential problem regarding reset.  Also may have to do with character stats and playeermovement
    {

        if (characterStats == null)
            characterStats = GetComponent<CharacterStats>();

        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();

        if (battleManager == null)
            battleManager = FindFirstObjectByType<BattleStateManager>();
        if (turnManager == null)
        {
            
            turnManager = FindFirstObjectByType<TurnManager>();
            return;
        }

        if (movementText == null)
        {
            var found = GameObject.Find("MovementNum");
            if (found != null)
                movementText = found.GetComponent<TMP_Text>();
        }
        BeginBattleMovement();
        TrackMovement();
        UpdateUI();
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
        {
           
            return;
        }
           

        if (turnManager == null)
        {
            Debug.Log("turn manager");
            return;
        }
            

        // Detect start of player turn
        if (turnManager.isPlayerTurn && !wasPlayerTurn)
            ResetMovementForNewTurn();

        wasPlayerTurn = turnManager.isPlayerTurn;

        // Disable movement if not player turn
        if (!turnManager.isPlayerTurn)
        {
            playerMovement.canMove = false;
            Debug.Log("Im here - in playermovement in line 66");
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
        Debug.Log("Im here - in playermovement in line 80");

    }

    void EndBattleMovement()
    {
        playerMovement.canMove = true;
        Debug.Log("Im here - in playermovement in line 89");
        if (movementText != null)
            movementText.text = "";
    }

    void ResetMovementForNewTurn()
    {
        characterStats.ResetMovement();
        lastPosition = transform.position;
        Debug.Log("Im here - in playermovement in line 98");
        playerMovement.canMove = true;
        UpdateUI();
        
    }

    void TrackMovement()
    {
        Debug.Log("trackmovement");
        float moved = Vector3.Distance(transform.position, lastPosition);

        if (moved > 0f)
        {
            characterStats.currentMovement -= moved;

            if (characterStats.currentMovement <= 0f)
            {
                characterStats.currentMovement = 0f;
                playerMovement.canMove = false;
                Debug.Log("Im here - in playermovement in line 116");
            }

            lastPosition = transform.position;
        }

        // If an ability restored movement, unlock movement again
        if (characterStats.currentMovement > 0f && turnManager.isPlayerTurn)
        {
            playerMovement.canMove = true;
            Debug.Log("Im here - in playermovement in line 121");
        }
    }


    void UpdateUI()
    {
        if (movementText == null) return;
        movementText.text =
            $"{characterStats.currentMovement:F2}m / {characterStats.maxMovement:F2}m";
    }
   
}

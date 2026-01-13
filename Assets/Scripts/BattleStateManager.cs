using System.Collections.Generic;
using UnityEngine;

public class BattleStateManager : MonoBehaviour
{
    [Header("Battle UI References")]
    public GameObject battleUI;
    public bool isBattleActive = false;

    private BattleIntroManager introManager;
    private TurnManager turnManager;

    [Header("Post-Battle Grace Period")]
    public float postBattleGraceTime = 2f;
    private float graceTimer = 0f;

    // Tracks overworld enemies that participated in current battle.
    private List<GameObject> currentBattleEnemies = new List<GameObject>();

    // Tracks ONLY enemies that actually died during battle.
    private HashSet<GameObject> killedEnemies = new HashSet<GameObject>();


    void Start()
    {
        if (battleUI != null)
            battleUI.SetActive(false);

        introManager = FindFirstObjectByType<BattleIntroManager>();
        turnManager = GetComponent<TurnManager>();
    }


    void Update()
    {
        if (!isBattleActive && graceTimer > 0f)
            graceTimer -= Time.deltaTime;
    }


    // -----------------------------------------------------------------
    // MARK ENEMY AS KILLED
    // -----------------------------------------------------------------
    public void MarkEnemyKilled(GameObject enemy)
    {
        if (enemy != null)
            killedEnemies.Add(enemy);
    }


    // -----------------------------------------------------------------
    // START BATTLE
    // -----------------------------------------------------------------
    public void StartBattle(List<GameObject> enemyGroup)
    {
        currentBattleEnemies = enemyGroup;
        killedEnemies.Clear();

        isBattleActive = true;

        if (battleUI != null)
            battleUI.SetActive(true);

        TriggerBattleIntro();

        // Find active player
        var partyController = FindFirstObjectByType<PlayerPartyController>();
        GameObject activePlayer = partyController != null ?
                                  partyController.activeMember : null;

        List<GameObject> players = new();
        if (activePlayer != null)
            players.Add(activePlayer);

        if (turnManager != null)
            turnManager.InitializeTurnOrder(players, enemyGroup);
    }


    public void StartBattleForGroup(List<GameObject> enemyGroup)
    {
        StartBattle(enemyGroup);
    }


    // -----------------------------------------------------------------
    // BATTLE INTRO 
    // -----------------------------------------------------------------
    private void TriggerBattleIntro()
    {
        if (introManager != null)
            introManager.PlayBattleIntro();
    }


    // -----------------------------------------------------------------
    // END BATTLE
    // -----------------------------------------------------------------
    public void EndBattle()
    {
        isBattleActive = false;
        graceTimer = postBattleGraceTime;

        if (battleUI != null)
            battleUI.SetActive(false);

        // Hide the Turn Order UI
        var turnUI = FindFirstObjectByType<TurnOrderUI>(FindObjectsInactive.Include);
        turnUI?.ClearUI();
        ResetAbilityBattleUses();

        CleanupDefeatedEnemies();
    }



    public bool IsInGracePeriod()
    {
        return graceTimer > 0f;
    }


    // -----------------------------------------------------------------
    // CLEAN ONLY THE ENEMIES THAT ACTUALLY DIED
    // -----------------------------------------------------------------
    private void CleanupDefeatedEnemies()
    {
        foreach (var enemy in currentBattleEnemies)
        {
            if (enemy == null) continue;

            if (killedEnemies.Contains(enemy))
            {
                // Detach children so they don’t disappear 
                foreach (Transform child in enemy.transform)
                    child.SetParent(null);

                Destroy(enemy.gameObject);
            }
        }

        currentBattleEnemies.Clear();
        killedEnemies.Clear();
    }
    private void ResetAbilityBattleUses()
    {
        Ability[] allAbilities = Resources.FindObjectsOfTypeAll<Ability>();

        foreach (var ability in allAbilities)
            ability.usesThisBattle = 0;
    }

}

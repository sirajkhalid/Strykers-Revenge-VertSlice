using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerPartyController : MonoBehaviour
{
    [Header("Party Setup")]
    public List<GameObject> partyPrefabs = new List<GameObject>();

    [Header("Runtime Party Objects")]
    public List<GameObject> partyMembers = new List<GameObject>();

    public int activeIndex = 0;
    public GameObject activeMember;

    [Header("References")]
    public Camera mainCamera;

    [Header("Settings")]
    public float spawnOffsetY = 0.0f;

    public CinemachineImpulseSource impulseSource;
    public bool disableSwitching = false;

    void Start()
    {
        if (!mainCamera)
            mainCamera = Camera.main;

        SpawnInitialParty();

        var ui = FindFirstObjectByType<TeamSwitchUI>();
        if (ui != null)
            ui.RefreshDisplay();

        Invoke(nameof(ForceHUDInit), 0.1f);
    }

    void SpawnInitialParty()
    {
        if (partyPrefabs.Count == 0)
        {
            Debug.LogError("No party prefabs assigned!");
            return;
        }

        Vector3 startPos = transform.position;

        
        GameObject first = Instantiate(partyPrefabs[0], startPos, Quaternion.identity);
        activeMember = first;
        partyMembers.Add(first);

        // Others inactive
        for (int i = 1; i < partyPrefabs.Count; i++)
        {
            GameObject p = Instantiate(partyPrefabs[i], startPos, Quaternion.identity);
            p.SetActive(false);
            partyMembers.Add(p);
        }

        // Init stats
        foreach (var m in partyMembers)
        {
            var stats = m.GetComponent<CharacterStats>();
            if (stats != null)
            {
                stats.CalculateAllStats();
                stats.currentHealth = stats.maxHealth;
            }
        }

        HookCamera();
    }

    void HookCamera()
    {
        var cine = FindFirstObjectByType<CinemachineCamera>();
        if (cine != null && activeMember)
            cine.Follow = activeMember.transform;
    }

    // SWITCH FUNCTION
    public void SwitchTo(int newIndex)
    {
        if (disableSwitching)
            return;
        if (newIndex < 0 || newIndex >= partyMembers.Count)
            return;
        if (newIndex == activeIndex)
            return;

        GameObject oldChar = activeMember;
        if (!oldChar) return;

        CharacterStats oldStats = oldChar.GetComponent<CharacterStats>();
        if (!oldStats) return;

        GameObject newChar = partyMembers[newIndex];
        if (!newChar) return;

        CharacterStats newStats = newChar.GetComponent<CharacterStats>();
        if (!newStats) return;

        if (newStats.currentHealth <= 0)
            return;

        // -------------------------------
        // BATTLE + TURN CONTEXT
        // -------------------------------
        var battle = FindFirstObjectByType<BattleStateManager>();
        var turnManager = FindFirstObjectByType<TurnManager>();
        bool inBattle = battle && battle.isBattleActive;

        // If in battle, switching costs a BONUS ACTION.
        // If player has no bonus OR already switched this round -> cannot switch.
        if (inBattle)
        {
            if (!oldStats.hasBonusAction || oldStats.hasSwitchedThisRound)
            {
                // Optional feedback
                oldStats.ShowFloatingText("No bonus action to switch!", Color.yellow);
                return;
            }
        }

        // POSITION + VFX
        Transform anchor = oldChar.transform.childCount > 0
            ? oldChar.transform.GetChild(0)
            : oldChar.transform;

        Vector3 pos = anchor.position;
        pos.y += spawnOffsetY;

        if (oldStats.swapOutVFX)
        {
            GameObject vfx = Instantiate(oldStats.swapOutVFX, anchor.position, Quaternion.identity);
            Destroy(vfx, oldStats.vfxLifetime);
        }


        if (!oldStats.isSneaking)
        {
            oldStats.isImmune = false;

            SpriteRenderer oldSR = oldChar.GetComponent<SpriteRenderer>();
            if (oldSR != null)
                oldSR.color = new Color(oldSR.color.r, oldSR.color.g, oldSR.color.b, 1f);
        }

        // Disable old character
        oldChar.SetActive(false);

        // Enable new one
        newChar.transform.position = pos;
        newChar.SetActive(true);

        if (inBattle)
            newStats.midSwapEnteredTurn = true;

        if (inBattle && turnManager != null)
            turnManager.ReplaceCombatant(oldChar, newChar);

        if (impulseSource)
            impulseSource.GenerateImpulse();

        if (newStats.swapInVFX)
        {
            GameObject vfx = Instantiate(newStats.swapInVFX, pos, Quaternion.identity);
            Destroy(vfx, newStats.vfxLifetime);
        }

        // SET NEW ACTIVE MEMBER
        activeMember = newChar;
        activeIndex = newIndex;
        HookCamera();


        if (newStats.isSneaking)
        {
            newStats.isSneaking = false;
            newStats.isImmune = false;

            SpriteRenderer sr = activeMember.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f);
        }

        // HUD/Portrait UI Updates
        var hud = FindFirstObjectByType<PlayerHUDManager>(FindObjectsInactive.Include);
        if (hud)
        {
            hud.SetTarget(newStats);
            hud.RefreshSkillBar(newStats);
        }

        var teamUI = FindFirstObjectByType<TeamSwitchUI>();
        if (teamUI)
        {
            teamUI.RefreshDisplay();
            teamUI.PlayPortraitSelectFX(newIndex);
        }

        // -------------------------------
        // MID-BATTLE TURN LOGIC
        // -------------------------------
        if (inBattle && turnManager != null)
        {
            // Old character spends BONUS ACTION to switch
            oldStats.hasBonusAction = false;
            oldStats.hasSwitchedThisRound = true;
            // Do NOT touch oldStats.hasAction here.

            // New character:
            // - ALWAYS gets an action
            // - NO bonus action (because switch consumed it)
            // - FULL movement
            newStats.hasAction = true;
            newStats.hasBonusAction = false;
            newStats.currentMovement = newStats.maxMovement;

            turnManager.currentTurnObject = newChar;
            turnManager.isPlayerTurn = true;

            if (hud)
                hud.UpdateActionUI();

            var battleUI = FindFirstObjectByType<BattleUIManager>(FindObjectsInactive.Include);
            if (battleUI)
                battleUI.ShowTurnBanner(newStats.characterName);
        }
    }


    // Next alive (switch on death)
    public bool HasAliveBackup()
    {
        foreach (var member in partyMembers)
        {
            if (member != activeMember &&
                member.GetComponent<CharacterStats>().currentHealth > 0)
                return true;
        }
        return false;
    }

    public CharacterStats GetActiveStats()
    {
        if (activeMember == null) return null;
        return activeMember.GetComponent<CharacterStats>();
    }
    public void SwitchToNextAlive()
    {
        for (int i = 0; i < partyMembers.Count; i++)
        {
            if (i != activeIndex)
            {
                var stats = partyMembers[i].GetComponent<CharacterStats>();
                if (stats.currentHealth > 0)
                {
                    
                    SwitchTo(i);
                    return;
                }
            }
        }
    }

    void ForceHUDInit()
    {
        var hud = FindFirstObjectByType<PlayerHUDManager>(FindObjectsInactive.Include);
        if (hud)
        {
            hud.SetTarget(activeMember.GetComponent<CharacterStats>());
            hud.RefreshSkillBar(activeMember.GetComponent<CharacterStats>());
        }
    }

    public void NotifyMemberDied(CharacterStats deadStats)
    {
        GameObject deadGO = deadStats.gameObject;

        // Disable sprite (not entire object)
        SpriteRenderer sr = deadGO.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.enabled = false;

        // Retarget all enemies
        foreach (var ai in FindObjectsByType<EnemyAIController>(
                     FindObjectsInactive.Include,
                     FindObjectsSortMode.None))
        {
            if (activeMember != null)
                ai.ForceRetarget(activeMember.transform);
        }

        // If the dead one wasn't the active member, nothing else to switch
        if (deadGO != activeMember)
            return;

        // If there is another alive party member, switch
        if (HasAliveBackup())
        {
            SwitchToNextAlive();

            // After switching, retarget again
            foreach (var ai in FindObjectsByType<EnemyAIController>(
                         FindObjectsInactive.Include,
                         FindObjectsSortMode.None))
            {
                ai.ForceRetarget(activeMember.transform);
            }
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameOver");
        }
    }
    public void TriggerSprintCameraShake()
    {
        if (impulseSource != null)
            impulseSource.GenerateImpulse();
    }



}

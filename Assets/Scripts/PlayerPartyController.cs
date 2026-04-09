using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerPartyController : MonoBehaviour
{
    public static PlayerPartyController Instance { get; private set; }

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

    public string currentScene = "MyScene";
    public string currentStartPoint = "StartPoint";
    public string NotificationText = "";

    SpriteRenderer spriteRenderer;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        if (!mainCamera)
            mainCamera = Camera.main;

        CleanupNullMembers();

        //  (prevents extra clones on new scene loads)
        if (partyMembers.Count == 0 || partyMembers.TrueForAll(m => m == null))
            SpawnInitialParty();
        else
            EnsureActiveMemberValid();

        CleanupNullMembers();
        EnsureActiveMemberValid();
        HookCamera();
        RefreshUI();
        Invoke(nameof(ForceHUDInit), 0.1f);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!mainCamera)
            mainCamera = Camera.main;

        CleanupNullMembers();
        EnsureActiveMemberValid();
        HookCamera();
        RefreshUI();
        Invoke(nameof(ForceHUDInit), 0.1f);
      
    }

    public void LoadGameData()
    {
        currentScene = PlayerPrefs.GetString("MyGame", "MyScene");
        currentStartPoint = PlayerPrefs.GetString("MyGameStartPoint", "MySceneMyPoint");
    }
    public void SaveGameData()
    {
       // PlayerPrefs.SetString("MyGame-scene", current)
    }

    public void ResetGameData()
    {
        
    }

    void CleanupNullMembers()
    {
        partyMembers.RemoveAll(m => m == null);
    }

    void EnsureActiveMemberValid()
    {
        if (activeMember != null) return;

        if (partyMembers.Count > 0)
        {
            activeMember = partyMembers[Mathf.Clamp(activeIndex, 0, partyMembers.Count - 1)];
            if (activeMember == null)
            {
                activeIndex = 0;
                activeMember = partyMembers[0];
            }
        }
    }

    void RefreshUI()
    {
        var ui = FindFirstObjectByType<TeamSwitchUI>(FindObjectsInactive.Include);
        if (ui != null)
            ui.RefreshDisplay();
    }

    void SpawnInitialParty()
    {
        if (partyPrefabs.Count == 0)
        {
            Debug.LogError("No party prefabs assigned!");
            return;
        }

        Vector3 startPos = transform.position;

        partyMembers.Clear();

        // First member = active
        GameObject first = Instantiate(partyPrefabs[0], startPos, Quaternion.identity);
        DontDestroyOnLoad(first);

        activeMember = first;
        activeIndex = 0;
        partyMembers.Add(first);

        // Others inactive
        for (int i = 1; i < partyPrefabs.Count; i++)
        {
            GameObject p = Instantiate(partyPrefabs[i], startPos, Quaternion.identity);
            DontDestroyOnLoad(p);

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
        var cine = FindFirstObjectByType<CinemachineCamera>(FindObjectsInactive.Include);
        if (cine != null && activeMember)
            cine.Follow = activeMember.transform;
    }

    // SWITCH FUNCTION
    public void SwitchTo(int newIndex)
    {
        if (disableSwitching)
            return;

        CleanupNullMembers();

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
        var battle = FindFirstObjectByType<BattleStateManager>(FindObjectsInactive.Include);
        var turnManager = FindFirstObjectByType<TurnManager>(FindObjectsInactive.Include);
        bool inBattle = battle && battle.isBattleActive;

        // If in battle, switching costs a BONUS ACTION.
        if (inBattle)
        {
            if (!oldStats.hasBonusAction || oldStats.hasSwitchedThisRound)
            {
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

        var teamUI = FindFirstObjectByType<TeamSwitchUI>(FindObjectsInactive.Include);
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
            oldStats.hasBonusAction = false;
            oldStats.hasSwitchedThisRound = true;

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
        CleanupNullMembers();

        foreach (var member in partyMembers)
        {
            if (member == null) continue;

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
        CleanupNullMembers();

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
        if (activeMember == null) return;

        var hud = FindFirstObjectByType<PlayerHUDManager>(FindObjectsInactive.Include);
        if (hud)
        {
            var cs = activeMember.GetComponent<CharacterStats>();
            if (cs != null)
            {
                hud.SetTarget(cs);
                hud.RefreshSkillBar(cs);
            }
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

        if (HasAliveBackup())
        {
            SwitchToNextAlive();

            foreach (var ai in FindObjectsByType<EnemyAIController>(
                         FindObjectsInactive.Include,
                         FindObjectsSortMode.None))
            {
                if (activeMember != null)
                    ai.ForceRetarget(activeMember.transform);
            }
        }
        else
        {
            SceneManager.LoadScene("GameOver");
        }
    }

    public void TriggerSprintCameraShake()
    {
        if (impulseSource != null)
            impulseSource.GenerateImpulse();
    }
}
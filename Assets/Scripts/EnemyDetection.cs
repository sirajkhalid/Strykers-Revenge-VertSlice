using System.Collections.Generic;
using UnityEngine;

public enum EnemyDetectionType
{
    Radius,
    VisionCone,
    RadiusAndCone
}

public class EnemyDetection : MonoBehaviour
{
    [Header("Detection Settings")]
    public EnemyDetectionType detectionType = EnemyDetectionType.Radius;
    public float radiusRange = 5f;

    [Header("Vision Cone")]
    public float coneHalfAngle = 45f;
    public float coneDistance = 6f;

    [Header("Battle Trigger")]
    public float battleTriggerRange = 2f;

    private EnemyMovementController mover;
    private EnemyGroupController groupController;
    private BattleStateManager battleManager;
    private PlayerPartyController party;

    private Transform player;
    private bool spottedPlayer = false;
    private bool battleStarted = false;

    void Awake()
    {
        // Followers cannot detect — automatic
        if (transform.parent != null &&
            transform.parent.GetComponent<EnemyMovementController>() != null)
        {
            enabled = false;
            return;
        }

        mover = GetComponent<EnemyMovementController>();
        groupController = GetComponent<EnemyGroupController>();
        battleManager = FindFirstObjectByType<BattleStateManager>();
        party = FindFirstObjectByType<PlayerPartyController>();
    }

    void Update()
    {
        if (battleManager != null && battleManager.isBattleActive) return;
        if (battleManager != null && battleManager.IsInGracePeriod()) return;
        if (battleStarted) return;

        if (party == null || party.activeMember == null) return;
        player = party.activeMember.transform;

        bool detected = detectionType switch
        {
            EnemyDetectionType.Radius => CheckRadius(),
            EnemyDetectionType.VisionCone => CheckCone(),
            EnemyDetectionType.RadiusAndCone => CheckRadius() || CheckCone(),
            _ => false
        };

        if (detected)
            HandleDetection();
    }

    bool CheckRadius()
    {
        return Vector3.Distance(transform.position, player.position) <= radiusRange;
    }

    bool CheckCone()
    {
        Vector3 dir = player.position - transform.position;
        if (dir.magnitude > coneDistance) return false;

        float angle = Vector2.Angle(transform.up, dir.normalized);
        return angle <= coneHalfAngle;
    }

    void HandleDetection()
    {
        if (!spottedPlayer)
        {
            spottedPlayer = true;
            mover?.ForceChasePlayer();
        }

        if (Vector3.Distance(transform.position, player.position) <= battleTriggerRange)
            StartBattle();
    }

    void StartBattle()
    {
        if (battleStarted) return;
        battleStarted = true;

        if (groupController != null)
            groupController.StopGroupMovement();
        else
            mover?.OnBattleStarted();

        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        if (pm != null)
            pm.canMove = false;

        List<GameObject> battleEnemies = new() { gameObject };

        if (groupController != null)
        {
            foreach (var f in groupController.followers)
                if (f != null)
                    battleEnemies.Add(f.gameObject);
        }

        battleManager?.StartBattleForGroup(battleEnemies);
    }
}

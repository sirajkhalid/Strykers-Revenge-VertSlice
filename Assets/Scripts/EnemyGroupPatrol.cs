using UnityEngine;

public class EnemyGroupPatrol : MonoBehaviour
{
    [Header("Patrol")]
    public Transform[] waypoints;          // Patrol path
    public float speed = 2f;
    public float waypointThreshold = 0.1f;

    [Header("Detection")]
    public float detectionRange = 3f;      // How close the active hero needs to be

    [Header("Battle")]
    public BattleStateManager battleStateManager; // Hook in inspector or auto-find

    private int currentWaypointIndex = 0;
    private bool isPatrolling = true;

    private PlayerPartyController partyController;

    void Start()
    {
        // Get references at runtime
        partyController = FindFirstObjectByType<PlayerPartyController>();
        if (battleStateManager == null)
            battleStateManager = FindFirstObjectByType<BattleStateManager>();
    }

    void Update()
    {
        if (!isPatrolling)
            return;

        Patrol();
        DetectPlayer();
    }

    void Patrol()
    {
        if (waypoints == null || waypoints.Length == 0)
            return;

        Transform targetWaypoint = waypoints[currentWaypointIndex];

        Vector3 direction = (targetWaypoint.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        if (Vector3.Distance(transform.position, targetWaypoint.position) < waypointThreshold)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }

    void DetectPlayer()
    {
        if (partyController == null || partyController.activeMember == null)
            return;

        Transform activePlayer = partyController.activeMember.transform;

        if (Vector3.Distance(transform.position, activePlayer.position) <= detectionRange)
        {
            isPatrolling = false;

            // Stop the active hero’s overworld movement
            PlayerMovement playerMovement = activePlayer.GetComponent<PlayerMovement>();
            if (playerMovement != null)
                playerMovement.canMove = false;

            // Start the battle
            if (battleStateManager != null)
                battleStateManager.StartBattle();
        }
    }
}

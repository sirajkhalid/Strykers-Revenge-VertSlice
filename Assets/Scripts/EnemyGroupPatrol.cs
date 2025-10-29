using UnityEngine;

public class EnemyGroupPatrol : MonoBehaviour
{
    public Transform[] waypoints; // Array of waypoints for the patrol path
    public float speed = 2f; // Speed of the enemies
    public float waypointThreshold = 0.1f; // Distance to consider a waypoint reached
    public float detectionRange = 3f; // Distance at which the player is detected
    public Transform player; // Reference to the player character
    public BattleStateManager battleStateManager; // Reference to the BattleStateManager

    private int currentWaypointIndex = 0;
    private bool isPatrolling = true;

    void Update()
    {
        if (isPatrolling)
        {
            Patrol();
            DetectPlayer();
        }
    }

    void Patrol()
    {
        // Get the current waypoint position
        Transform targetWaypoint = waypoints[currentWaypointIndex];

        // Move towards the current waypoint
        Vector3 direction = (targetWaypoint.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        // Check if the group has reached the current waypoint
        if (Vector3.Distance(transform.position, targetWaypoint.position) < waypointThreshold)
        {
            // Move to the next waypoint
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }

    void DetectPlayer()
    {
        // Check if the player is within detection range
        if (Vector3.Distance(transform.position, player.position) <= detectionRange)
        {
            isPatrolling = false; // Stop patrolling
            PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.canMove = false; // Stop player movement
            }

            // Trigger battle state
            battleStateManager.ToggleBattleState();
        }
    }
}
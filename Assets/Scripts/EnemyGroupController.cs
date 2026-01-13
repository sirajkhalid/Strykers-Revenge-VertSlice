using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyGroupController : MonoBehaviour
{
    [Header("Followers (auto-filled)")]
    public List<EnemyMovementController> followers = new List<EnemyMovementController>();

    [Header("Formation Settings")]
    public float baseRadius = 1.5f;
    public float jitterAmount = 0.3f;
    public float jitterSpeed = 1.5f;

    private EnemyMovementController leader;

    private List<Vector3> baseOffsets = new();
    private List<Vector3> currentOffsets = new();
    private List<Vector3> jitterTargets = new();

    private bool groupStopped = false;

    void Awake()
    {
        
        if (transform.parent != null &&
            transform.parent.GetComponent<EnemyMovementController>() != null)
        {
            enabled = false;
            return;
        }

        // This object itself must have EnemyMovementController to be a leader
        leader = GetComponent<EnemyMovementController>();
        if (leader == null)
        {
            enabled = false;
            return;
        }
    }

    void Start()
    {
        // ONLY direct children = followers
        followers = transform
            .GetComponentsInChildren<EnemyMovementController>()
            .Where(e => e != leader && e.transform.parent == transform)
            .ToList();

        InitializeFollowers();
    }

    void LateUpdate()
    {
        if (groupStopped) return;
        if (leader == null) return;

        Vector3 leaderPos = leader.transform.position;

        for (int i = 0; i < followers.Count; i++)
        {
            var follower = followers[i];
            if (follower == null) continue;

            
            UpdateFollowerOffset(i);

            // Target position = leader + current offset
            Vector3 target = leaderPos + currentOffsets[i];
            follower.ReceiveFollowTarget(target);
            follower.chaseSpeedMultiplier = 1.0f;
        }
    }

    private void InitializeFollowers()
    {
        baseOffsets.Clear();
        currentOffsets.Clear();
        jitterTargets.Clear();

        int count = followers.Count;
        float angleStep = count > 0 ? 360f / count : 0f;

        for (int i = 0; i < count; i++)
        {
            var follower = followers[i];

            if (follower == null)
            {
                baseOffsets.Add(Vector3.zero);
                currentOffsets.Add(Vector3.zero);
                jitterTargets.Add(Vector3.zero);
                continue;
            }

            Vector3 offset = follower.transform.position - leader.transform.position;

            // If they start stacked, give them a radial spot
            if (offset.magnitude < 0.1f)
            {
                float angle = angleStep * i * Mathf.Deg2Rad;
                offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * baseRadius;
            }

            baseOffsets.Add(offset);
            currentOffsets.Add(offset);
            jitterTargets.Add(offset + Random.insideUnitSphere * jitterAmount);
        }
    }

    private void UpdateFollowerOffset(int index)
    {
        if (Vector3.Distance(currentOffsets[index], jitterTargets[index]) < 0.05f)
        {
            Vector3 baseOffset = baseOffsets[index];
            Vector2 rand = Random.insideUnitCircle * jitterAmount;
            jitterTargets[index] = baseOffset + new Vector3(rand.x, rand.y, 0f);
        }

        currentOffsets[index] = Vector3.Lerp(
            currentOffsets[index],
            jitterTargets[index],
            Time.deltaTime * jitterSpeed
        );
    }

    public void StopGroupMovement()
    {
        if (leader != null)
            leader.OnBattleStarted();

        foreach (var f in followers)
            if (f != null)
                f.OnBattleStarted();

        groupStopped = true;
    }
}

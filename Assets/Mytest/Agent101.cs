using UnityEngine;
using UnityEngine.AI;


public class Agent101 : MonoBehaviour
{
    private NavMeshAgent _agent;

    public GameObject Player;

    public float EnemyDistanceRun = 4.0f;


    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = false;
        _agent.updateUpAxis = false;
    }

    // Update is called once per frame
    void Update()
    {
        float distance = Vector3.Distance(transform.position, Player.transform.position);

        Debug.Log("Distance " + distance);

        if (distance > EnemyDistanceRun)
        {
            Vector3 dirToPlayer = transform.position - Player.transform.position;

            Vector3 newPos = transform.position - dirToPlayer;

            _agent.SetDestination(newPos);
        }
    }
}

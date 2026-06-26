using UnityEngine;
using UnityEngine.AI;

public class Alien : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;
    private GameObject player;
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (player != null)
        {
            navMeshAgent.SetDestination(player.transform.position);
            Debug.Log("Chasing player at: " + player.transform.position);
        }
        else
        {
            Debug.LogError("Alien cannot find a GameObject with the tag 'Player'!");
        }
    }
}

using UnityEngine;
using UnityEngine.AI;

public class Alien : MonoBehaviour
{
    public float health = 100;

    [Header("Movement & Pathfinding")]
    private NavMeshAgent navMeshAgent;
    private GameObject player;
    private PlayerManager playerManager;

    [Header("Attack Settings")]
    public float attackRadius = 2.0f;

    private Animator animator;
    private bool isBiting = false;
    private bool isDead = false;

    void Start()
    {
        player = GameObject.FindWithTag("Player");
        navMeshAgent = GetComponent<NavMeshAgent>();

        if (player != null)
        {
            playerManager = player.GetComponent<PlayerManager>();
        }

        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (isDead || navMeshAgent == null || !navMeshAgent.enabled || !navMeshAgent.isOnNavMesh) return;

        if (player == null)
        {
            Debug.LogError("Alien cannot find a GameObject with the tag 'Player'!");
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        if (distanceToPlayer <= attackRadius)
        {
            navMeshAgent.isStopped = true;

            if (!isBiting)
            {
                TriggerBite();
            }
        }
        else
        {
            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(player.transform.position);
            isBiting = false;
        }
    }

    void TriggerBite()
    {
        if (animator != null && !isDead)
        {
            animator.SetTrigger("Bite");
            isBiting = true;
            if (playerManager != null)
            {
                playerManager.SpawnBlood();
                playerManager.StopMovement();
            }
            Invoke("triggerPlayerDeath", 2f);
        }
    }

    void triggerPlayerDeath()
    {
        if (playerManager != null)
        {
            playerManager.Die();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        health -= amount;
        if (health <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;

        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.AddKill();
        }

        if (navMeshAgent != null && navMeshAgent.enabled && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.enabled = false;
        }

        Collider alienCollider = GetComponent<Collider>();
        if (alienCollider != null)
        {
            alienCollider.enabled = false;
        }

        if (animator != null)
        {
            animator.SetTrigger("Death");
        }

        Destroy(gameObject, 3.0f);
    }
}
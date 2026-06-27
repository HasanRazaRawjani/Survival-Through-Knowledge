using UnityEngine;
using System.Collections;

public class DropshipController : MonoBehaviour
{
    public GameObject[] zombiePrefabs;
    public Transform spawnLocation;
    public float minSpawnDelay = 3f;
    public float maxSpawnDelay = 8f;
    public bool spawnOnGround = true;

    public float flightSpeed = 15f;
    public float rotationSpeed = 3f;
    public Vector3 patrolAreaSize = new Vector3(100f, 20f, 100f);

    private Vector3 spawnOrigin;
    private Vector3 currentFlightTarget;
    private int lastZombieIndex = -1;

    void Start()
    {
        spawnOrigin = transform.position;
        PickNewRandomTarget();

        if (zombiePrefabs != null && zombiePrefabs.Length > 0)
        {
            StartCoroutine(ZombieSpawnRoutine());
        }
    }

    void Update()
    {
        FlyTowardsTarget();
    }

    void FlyTowardsTarget()
    {
        Vector3 direction = (currentFlightTarget - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
        }

        transform.position = Vector3.MoveTowards(transform.position, currentFlightTarget, flightSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, currentFlightTarget) < 2.0f)
        {
            PickNewRandomTarget();
        }
    }

    void PickNewRandomTarget()
    {
        float randomX = Random.Range(-patrolAreaSize.x / 2f, patrolAreaSize.x / 2f);
        float randomY = Random.Range(-patrolAreaSize.y / 2f, patrolAreaSize.y / 2f);
        float randomZ = Random.Range(-patrolAreaSize.z / 2f, patrolAreaSize.z / 2f);

        currentFlightTarget = spawnOrigin + new Vector3(randomX, randomY, randomZ);
    }

    IEnumerator ZombieSpawnRoutine()
    {
        while (true)
        {
            float randomDelay = Random.Range(minSpawnDelay, maxSpawnDelay);
            yield return new WaitForSeconds(randomDelay);

            if (spawnLocation == null) continue;

            int randomZombieIndex = Random.Range(0, zombiePrefabs.Length);

            if (zombiePrefabs.Length > 1)
            {
                while (randomZombieIndex == lastZombieIndex)
                {
                    randomZombieIndex = Random.Range(0, zombiePrefabs.Length);
                }
            }

            lastZombieIndex = randomZombieIndex;
            GameObject chosenZombiePrefab = zombiePrefabs[randomZombieIndex];

            Vector3 finalSpawnPosition = spawnLocation.position;

            if (spawnOnGround)
            {
                if (Physics.Raycast(spawnLocation.position, Vector3.down, out RaycastHit hit, 500f))
                {
                    finalSpawnPosition = hit.point;
                }
            }

            Instantiate(chosenZombiePrefab, finalSpawnPosition, Quaternion.identity);
        }
    }

    private void OnDrawGizmos()
    {
        Vector3 boxCenter = Application.isPlaying ? spawnOrigin : transform.position;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(boxCenter, patrolAreaSize);

        Gizmos.color = new Color(0f, 1f, 1f, 0.05f);
        Gizmos.DrawCube(boxCenter, patrolAreaSize);
    }
}
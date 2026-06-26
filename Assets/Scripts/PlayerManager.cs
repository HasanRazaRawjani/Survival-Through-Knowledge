using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public GameObject bloodPrefab;
    public GameObject bloodSpawnLocation;
    private Rigidbody rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        StartMovement();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SpawnBlood()
    {

    }

    public void Die()
    {
        Destroy(gameObject);
    }

    public void StopMovement()
    {
        rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;
    }

    public void StartMovement()
    {
        rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;
    }

}

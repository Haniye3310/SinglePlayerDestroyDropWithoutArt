using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class Gift : MonoBehaviour
{
    public Rigidbody RigidBody;
    private Vector3 _moveDirection;
    private DataRepo dataRepo;
    public ItemType ItemType;
    public bool Consumed;


    private void Start()
    {
        RigidBody = GetComponent<Rigidbody>();

        _moveDirection = GetRandomDirection();
        // Disable default Unity gravity
        RigidBody.useGravity = false;
        this.RigidBody.AddForce(Vector3.up * 1, ForceMode.Impulse);
        dataRepo = FindAnyObjectByType<DataRepo>();

       
    }
    public static Vector3 GetRandomDirection()
    {
        // Randomly get a direction vector in the x-z plane
        Vector3 randomDirection = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0, UnityEngine.Random.Range(-1f, 1f));
        return randomDirection.normalized;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            RigidBody.useGravity = true;
            RigidBody.linearVelocity = new Vector3(this._moveDirection.x * 1f, 2, this._moveDirection.z * 1f);
        }
    }
    private void Update()
    {
        // Check if the gift has moved outside the cylinder
        if (Vector2.Distance(new Vector2( dataRepo.GroundCenter.x,dataRepo.GroundCenter.z)
            , new Vector2(transform.position.x, transform.position.z)) >= dataRepo.GroundRadius-0.2)
        {
            SystemFunction.RemoveGiftFromLists(dataRepo, this);
            // Destroy or disable the gift
 

            Destroy(gameObject);

        }
        

    }
    private void FixedUpdate()
    {
        if(RigidBody.useGravity == false)
            RigidBody.AddForce(Vector3.down * 3 * RigidBody.mass, ForceMode.Force);
    }
}

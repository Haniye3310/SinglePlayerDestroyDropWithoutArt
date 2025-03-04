using UnityEngine;

public class CharacterPartCollider : MonoBehaviour
{
    private DataRepo dataRepo;
    private Player player;
    private void Start()
    {
        dataRepo = FindAnyObjectByType<DataRepo>();
        player = GetComponentInParent<Player>();
    }
    private void OnCollisionEnter(Collision collision)
    {
        SystemFunction.OnPlayerCollisionEnter(this,player,collision,dataRepo);
    }
    private void OnCollisionExit(Collision collision)
    {
        SystemFunction.OnPlayerCollisionExit(player, collision, dataRepo);
    }
    private void OnCollisionStay(Collision collision)
    {
        SystemFunction.OnPlayerCollisionStay(player, collision, dataRepo);
    }
    private void OnTriggerEnter(Collider other)
    {
        SystemFunction.OnPlayerTriggerEnter(this,player,dataRepo,other);
    }
}

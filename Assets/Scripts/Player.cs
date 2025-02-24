using NUnit.Framework.Internal;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    private DataRepo dataRepo;
    public bool IsMainPlayer;
    public GameObject Visual;
    public TextMeshProUGUI playerScoreText;
    public Image starIconNearText;

    private void Start()
    {
        dataRepo = FindAnyObjectByType<DataRepo>();
    }
    private void OnCollisionEnter(Collision collision)
    {
        SystemFunction.OnPlayerCollisionEnter(this,this,collision,dataRepo);
    }
    private void OnTriggerEnter(Collider other)
    {
        SystemFunction.OnPlayerTriggerEnter(this,this,dataRepo,other);
    }
    private void OnCollisionExit(Collision collision)
    {
        SystemFunction.OnPlayerCollisionExit(this,collision,dataRepo);
    }
    private void OnCollisionStay(Collision collision)
    {
        SystemFunction.OnPlayerCollisionStay(this,collision,dataRepo);
    }
}

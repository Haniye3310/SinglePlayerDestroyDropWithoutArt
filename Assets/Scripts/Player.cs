using NUnit.Framework;
using NUnit.Framework.Internal;
using System.Collections.Generic;
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
    private void Update()
    {
        DetectCoins();
    }
    void DetectCoins()
    {
        List<Gift> gifts = new List<Gift>();
        gifts.AddRange(dataRepo.HammerList);
        foreach (GiftTime giftTime in dataRepo.CoinList)
        {
            gifts.Add(giftTime.Gift);

        }
        foreach (GiftTime giftTime in dataRepo.BagOfCoinList)
        {
            gifts.Add(giftTime.Gift);

        }
        foreach (Gift gift in gifts)
        {
            float distance = Vector3.Distance(transform.position, gift.transform.position);

            if (distance < 0.5f) // If the player is close enough
            {
                SystemFunction.CollectCoin(dataRepo,gift,this,this);
            }
        }
    }
}

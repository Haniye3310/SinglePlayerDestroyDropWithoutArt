using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MonoMesssgeReciever : MonoBehaviour
{
    public DataRepo DataRepo;
    bool start;
    private IEnumerator Start()
    {
        foreach (PlayerData p in DataRepo.Players)
        {
            p.PlayerAnimator.SetBool("Grounded", p.IsGrounded);
        }
        DataRepo.UIPanel.gameObject.SetActive(false);
        while (Mathf.Abs(Camera.main.fieldOfView - 60) > 0.1)
        {
            Camera.main.fieldOfView= Mathf.Lerp(Camera.main.fieldOfView,60,0.1f);
            yield return null;
        }

        DataRepo.StartCountDownTimer.gameObject.SetActive(true);
        float remaingStartTime = 4;
        while (remaingStartTime > 0)
        {
            remaingStartTime -= Time.deltaTime;
            if (((int)remaingStartTime + 1) == 4)
            {
                DataRepo.StartCountDownTimer.sprite = DataRepo.NumberThreeSprite;
            }
            if (((int)remaingStartTime + 1) == 3)
            {
                DataRepo.StartCountDownTimer.sprite = DataRepo.NumberTwoSprite;
            }
            if (((int)remaingStartTime + 1) == 2)
            {
                DataRepo.StartCountDownTimer.sprite = DataRepo.NumberOneSprite;
            }
            if (((int)remaingStartTime + 1) == 1)
            {
                DataRepo.StartCountDownTimer.gameObject.SetActive(false);
                DataRepo.GoImage.gameObject.SetActive(true);
            }
            yield return null;
        }
        DataRepo.GoImage.gameObject.SetActive(false);
        DataRepo.UIPanel.gameObject.SetActive(true);
        start = true;
        DataRepo.GroundCenter = DataRepo.GroundCollider.position;
        DataRepo.GroundRadius = DataRepo.GroundCollider.localScale.x / 2;
        DataRepo.GeneratorData.maxRadius = (DataRepo.GroundCollider.localScale.x / 4f);
        DataRepo.GeneratorData.minRadius = (DataRepo.GroundCollider.localScale.x / 5);


        DataRepo.GeneratorData.ItemTypeNumberPairs.Add(key: ItemType.Coin,value:8 );
        DataRepo.GeneratorData.ItemTypeNumberPairs.Add(key: ItemType.Hammer,value:5 );
        DataRepo.GeneratorData.ItemTypeNumberPairs.Add(key: ItemType.BagOfCoin,value:16 );

        StartCoroutine(SystemFunction.StartTimerOftheGame(DataRepo));
        StartCoroutine(SystemFunction.GenerateHammerOrCoin(DataRepo));
        StartCoroutine(SystemFunction.RotateGenerator(DataRepo));
        for (int i = 0; i< DataRepo.Players.Count; i++)
        {
            if (!DataRepo.Players[i].Player.IsMainPlayer)
            {
                StartCoroutine(SystemFunction.StartRobot(DataRepo.Players[i], DataRepo));
            }
        }
        StartCoroutine(SystemFunction.ChangeBotLevel(DataRepo));

    }
    private void FixedUpdate()
    {
        if(start)
            SystemFunction.FixedUpdate(DataRepo);
    }
    private void Update()
    {
        if (start)

            SystemFunction.Update(DataRepo);
    }
    public void OnRestartClicked()
    {
        SceneManager.LoadScene("FinalSceneWithBothArtAndLogic");
    }
    public void OnJumpClicked() 
    {
        foreach(PlayerData playerData in DataRepo.Players)
        {
            if (playerData.Player.IsMainPlayer)
            {
                SystemFunction.OnJumpClicked(DataRepo,playerData);

            }
        }
        
    }
}

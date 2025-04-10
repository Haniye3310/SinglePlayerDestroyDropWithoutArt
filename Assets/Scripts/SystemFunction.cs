using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Unity.Collections.Unicode;

public class SystemFunction
{
    public static IEnumerator StartTimerOftheGame(DataRepo dataRepo)
    {
        dataRepo.RemainingTimeInGame = dataRepo.TimeOftheGame;
        while (dataRepo.RemainingTimeInGame > 0)
        {
            dataRepo.RemainingTimeInGame -= Time.deltaTime;
            if((int)dataRepo.RemainingTimeInGame < 5)
            {
                dataRepo.TimerImage.sprite = dataRepo.TimerRedBG;
            }
            dataRepo.RemainingTimeText.text = ((int)dataRepo.RemainingTimeInGame).ToString();
            yield return null;
        }
        //stops the gameplay
        {
            dataRepo.ShouldStopGame = true;
        }
        
        dataRepo.ResultPanel.gameObject.SetActive(true);
        dataRepo.UIPanel.gameObject.SetActive(false);

        // Sort players by number of coins in descending order
        var sortedPlayers = dataRepo.Players.OrderByDescending(p => p.NumberOfCoin).ToList();

        // Find the main player's rank
        int mainPlayerRank = -1;
        for (int i = 0; i < sortedPlayers.Count; i++)
        {
            if (sortedPlayers[i].Player.IsMainPlayer)
            {
                mainPlayerRank = i + 1; // Ranks start at 1
                break;
            }
        }

        // Set the sprite based on the main player's rank
        switch (mainPlayerRank)
        {
            case 1:
                dataRepo.ResultPanelBG.sprite = dataRepo.FirstSprite;
                break;
            case 2:
                dataRepo.ResultPanelBG.sprite = dataRepo.SecondSprite;
                break;
            case 3:
                dataRepo.ResultPanelBG.sprite = dataRepo.ThirdSprite;
                break;
            case 4:
                dataRepo.ResultPanelBG.sprite = dataRepo.FourthSprite;
                break;
            default:
                Debug.LogError("Main player rank is out of range or not found.");
                break;
        }

    }
    public static void OnPlayerCollisionEnter(MonoBehaviour mono,Player player,Collision collision, DataRepo dataRepo)
    {
        PlayerData playerData = null;
        foreach (PlayerData p in dataRepo.Players)
        {
            if (p.Player == player)
            {
                playerData = p;
            }
        }

        if (collision.gameObject.tag == "Ground")
        {
            playerData.IsGrounded = true;
        }
        if (collision.gameObject.CompareTag("Gift"))
        {
            Gift gift = collision.gameObject.GetComponent<Gift>();

            if (gift.ItemType == ItemType.Coin)
            {
                if (!gift.Consumed)
                    mono.StartCoroutine(SystemFunction.AddCoin(playerData, 1));
            }
            if (gift.ItemType == ItemType.BagOfCoin)
            {
                if (!gift.Consumed)
                    mono.StartCoroutine(SystemFunction.AddCoin(playerData, 5));
            }
            if (gift.ItemType != ItemType.Hammer)
            {
                SystemFunction.RemoveGiftFromLists(dataRepo, gift);
                gift.Consumed = true;
                // Destroy or disable the gift
                GameObject.Destroy(gift.gameObject);
            }
        }

    }
    public static void OnPlayerCollisionStay(Player player, Collision collision, DataRepo dataRepo)
    {
        Player otherPlayer = null;
        if (collision.gameObject.tag == "Player")
        {
            otherPlayer = collision.gameObject.GetComponent<Player>();
        }
        PlayerData otherPlayerData = null;
        PlayerData playerData = null;
        foreach (PlayerData p in dataRepo.Players)
        {
            if (p.Player == otherPlayer)
            {
                otherPlayerData = p;
            }
            if (p.Player == player)
            {
                playerData = p;
            }
        }
        if (otherPlayer != null && playerData.IsGrounded && otherPlayerData.IsGrounded)
        {
            Vector3 pushDirection = (otherPlayer.transform.position - player.transform.position).normalized;
            float strengthDifference = playerData.Strength - otherPlayerData.Strength;

            if (strengthDifference > 0) // This character is stronger
            {
                ApplyPush(pushDirection, strengthDifference, otherPlayerData);
            }
            else if (strengthDifference < 0) // The other character is stronger
            {
                ApplyPush(-pushDirection, Mathf.Abs(strengthDifference), playerData);
            }
        }
    }
    public static void OnPlayerCollisionExit(Player player, Collision collision, DataRepo dataRepo)
    {

        if (collision.gameObject.tag == "Ground")
        {
            foreach (PlayerData p in dataRepo.Players)
            {
                if (p.Player == player)
                {
                    p.IsGrounded = false;
                }
            }
        }
    }
    public static void OnPlayerTriggerEnter(MonoBehaviour mono,Player player,DataRepo dataRepo, Collider other)
    {
        PlayerData playerData = null;
        foreach (PlayerData p in dataRepo.Players)
        {
            if (p.Player == player)
            {
                playerData = p;
            }
        }
        if (other.gameObject.CompareTag("Gift"))
        {
            Gift gift = other.gameObject.GetComponentInParent<Gift>();
            if (gift.ItemType == ItemType.Hammer)
            {
                if (!playerData.IsFrozen)
                {
                    mono.StartCoroutine(FreezePlayer(playerData));
                }
            }
            
        }

    }

    public static void Move(DataRepo dataRepo, PlayerData playerData, Vector3 direction)
    {

        if (playerData.IsFrozen) return;
        direction = direction.normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            playerData.PlayerRigidbody.AddForce
                                (direction * 75, ForceMode.Force);
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            playerData.PlayerRigidbody.
                MoveRotation(Quaternion.Slerp(playerData.PlayerRigidbody.rotation, targetRotation, Time.fixedDeltaTime * 10f));
        }
        if (direction.magnitude < 0.1f)
        {
            playerData.PlayerAnimator.SetFloat("MoveSpeed", 0);
        }
        else
        {
            playerData.PlayerAnimator.SetFloat("MoveSpeed", 1);
        }
    }
    public static void ApplyPush(Vector3 pushDirection, float forceAmount, PlayerData playerData)
    {
        playerData.PushForce = pushDirection * forceAmount * 5;
    }
    public static void FixedUpdate(DataRepo dataRepo)
    {
        float v = dataRepo.Joystick.Vertical;
        float h = dataRepo.Joystick.Horizontal;

        Vector3 direction = new Vector3(h,0,v) * -1;
        foreach(PlayerData p in  dataRepo.Players)
        {
            if (p.Player.IsMainPlayer)
            {
                Move(dataRepo,p,direction);

            }
            else
            {
                if (p.TargetItem)
                {
                    Vector3 targetCoinPosOnGround =
                        new Vector3
                        (p.TargetItem.position.x,
                        dataRepo.GroundCollider.position.y + (dataRepo.GroundCollider.localScale.y),
                        p.TargetItem.position.z);
                    Debug.DrawLine(p.TargetItem.position, p.Player.transform.position, Color.red);
                    Move(dataRepo, p, (targetCoinPosOnGround - p.Player.transform.position));
                }
                if (p.TargetAvoid)
                {
                    Vector3 targetAvoidPosOnGround =
                    new Vector3
                    (p.TargetAvoid.position.x,
                    dataRepo.GroundCollider.position.y + (dataRepo.GroundCollider.localScale.y),
                    p.TargetAvoid.position.z);
                    Debug.DrawLine(p.TargetMovement, p.Player.transform.position, Color.yellow);
                    Move(dataRepo, p, (p.Player.transform.position - targetAvoidPosOnGround));
                }

                if (!p.TargetAvoid && !p.TargetItem)
                {
                    Debug.DrawLine(p.TargetMovement, p.Player.transform.position, Color.green);
                    Move(dataRepo, p, (p.TargetMovement - p.Player.transform.position));
                }
            }
            if (p.ShouldJump)
            {
                Jump(dataRepo,p);
            }
            // Faster falling when in air
            if (!p.IsGrounded && p.PlayerRigidbody.linearVelocity.y < 0)
            {
                p.PlayerRigidbody.mass = 15;
            }
            else
            {
                p.PlayerRigidbody.mass = 1;
            }
            p.PlayerAnimator.SetBool("Grounded", p.IsGrounded);
        }
        foreach (PlayerData p in dataRepo.Players)
        {
            if (p.PushForce.magnitude > 0.01f) // Apply force smoothly
            {
                p.PlayerRigidbody.AddForce(p.PushForce, ForceMode.Acceleration);
                p.PushForce *= 0.9f; // Gradually reduce the push force to prevent infinite sliding
            }

            else
            {
                p.PushForce = Vector3.zero;
            }
        }
        if (dataRepo.ShouldStopGame)
        {
            foreach (PlayerData p in dataRepo.Players)
            {
                Move(dataRepo, p, Vector3.zero);
            }
        }
    }

    public static void OnJumpClicked(DataRepo dataRepo, PlayerData playerData)
    {
        playerData.ShouldJump = true;

        if (playerData == dataRepo.Players[1])
        {
            Debug.Log("OnJumpClicked");
        }

    }
    public static void Jump(DataRepo dataRepo,PlayerData playerData)
    {
        if (playerData == dataRepo.Players[1])
        {
            Debug.Log("Jump");
        }
        if (playerData.IsGrounded)
        {
            if (playerData == dataRepo.Players[1])
            {
                Debug.Log("IsGrounded Force");
            }

            playerData.PlayerRigidbody.AddForce(Vector3.up * 5, ForceMode.Impulse);
            playerData.ShouldJump = false;

        }
    }

    public static void Update(DataRepo dataRepo)
    {

        PlayerData mainPlayer = null;
        foreach (PlayerData p in dataRepo.Players)
        {
            // Skip if 'p' is the main player
            if (p.Player.IsMainPlayer)
            {
                mainPlayer = p;
                break;
            }
        }
        dataRepo.ShouldDropPrizeNearBots = false;
        dataRepo.ShouldDropPrizeNearPlayer = false;

        List<PlayerData> botlist = dataRepo.Players.Where(x => !x.Player.IsMainPlayer).ToList();
        PlayerData maxbot = botlist.OrderBy(x => x.NumberOfCoin).Last();

        // Check if main player's coin count is double of 'p'
        if (mainPlayer.NumberOfCoin > maxbot.NumberOfCoin)
        {
            dataRepo.ShouldDropPrizeNearBots = true;

        }

        // Check if 'p' has double the coins of the main player
        else if (mainPlayer.NumberOfCoin < maxbot.NumberOfCoin)
        {
            dataRepo.ShouldDropPrizeNearPlayer = true;

        }

    }
    public static IEnumerator ChangeBotLevel(DataRepo dataRepo)
    {
        while (true)
        {
            if(dataRepo.RemainingTimeInGame> 10 && dataRepo.RemainingTimeInGame < 20)
            {
                if (dataRepo.ShouldDropPrizeNearBots) 
                {
                    //make bots smarter
                    foreach(PlayerData p in dataRepo.Players)
                    {
                        if(p.BotLevel == BotLevel.Bad)
                        {
                            p.SecondToMakeDecision = 0.1f;
                            p.ChanceOfAvoidance = 20;
                            p.GiftPriority = ItemType.BagOfCoin;

                        }
                    }

                }
                if (dataRepo.ShouldDropPrizeNearPlayer)
                {
                    //make bots more stupid
                    foreach (PlayerData p in dataRepo.Players)
                    {
                        if(p.BotLevel == BotLevel.Good || p.BotLevel == BotLevel.Medium)
                        {
                            p.SecondToMakeDecision = 0.5f;
                            p.ChanceOfAvoidance = 60;
                            p.GiftPriority = ItemType.Coin;
                        }
                    }
                }
            }
            yield return null;
        }
    }
    public static IEnumerator GenerateHammerOrCoin(DataRepo dataRepo) 
    {
        float lastRegularDrop = -dataRepo.DropRegularThreshold;
        Random.InitState(System.DateTime.Now.Millisecond);
        while (true) 
        {
            if ((int)dataRepo.RemainingTimeInGame < 3) yield break;

            if(Time.time > lastRegularDrop+ dataRepo.DropRegularThreshold&&
                ( (int)dataRepo.RemainingTimeInGame >= 20
                || (int)dataRepo.RemainingTimeInGame <= 10))
            {
                lastRegularDrop = Time.time;


                // Randomly choose an item type based on probabilities
                var list = dataRepo.GeneratorData.ItemTypeNumberPairs.ToList();
                int rand = UnityEngine.Random.Range(0,list.Count);

                Gift go = null;
                ItemType typeToGenerate = default;
                int i = 0;
                while ( i < list.Count)
                {
                    if (list[rand].Value > 0)
                    {
                        typeToGenerate = list[rand].Key;
                        dataRepo.GeneratorData.ItemTypeNumberPairs[list[rand].Key]--;
                        break;
                    }
                    else
                    {
                        rand++;
                        if (rand == list.Count)
                            rand = 0;
                    }
                    i++;
                }

                InstantiateItemAndAddThemToLists(dataRepo, typeToGenerate);
                
            }

            if((int)dataRepo.RemainingTimeInGame <20  && (int)dataRepo.RemainingTimeInGame > 10)
            {

                if (dataRepo.ShouldDropPrizeNearBots || dataRepo.ShouldDropPrizeNearPlayer)
                {
                    Transform Mainplayer = null;
                    Transform BadBot = null;

                    for (int i = 0; i < dataRepo.Players.Count; i++)
                    {
                        if (dataRepo.Players[i].Player.IsMainPlayer)
                            Mainplayer = dataRepo.Players[i].Player.transform;

                        if (dataRepo.Players[i].BotLevel == BotLevel.Bad)
                            BadBot = dataRepo.Players[i].Player.transform;
                    }

                    Vector2 mainPlayerPosVector2 = new Vector2(Mainplayer.position.x, Mainplayer.position.z);
                    Vector2 badBotPosVector2 = new Vector2(BadBot.position.x, BadBot.position.z);

                    Vector2 generatorPosVector2 =
                        new Vector2(dataRepo.GeneratorData.Generator.transform.position.x, dataRepo.GeneratorData.Generator.transform.position.z);

                    if (Time.time > lastRegularDrop + dataRepo.DropRegularThreshold)
                    {
                        ItemType typeToGenerate = default;

                        if (Vector2.Distance(generatorPosVector2, mainPlayerPosVector2)
                            > Vector2.Distance(generatorPosVector2, badBotPosVector2))
                        {
                            if (dataRepo.ShouldDropPrizeNearPlayer) {typeToGenerate = ItemType.Hammer;}
                            if (dataRepo.ShouldDropPrizeNearBots){typeToGenerate = ItemType.None;}
                        }
                        else
                        {
                            if (dataRepo.ShouldDropPrizeNearPlayer){typeToGenerate = ItemType.None;}
                            if (dataRepo.ShouldDropPrizeNearBots){typeToGenerate = ItemType.Hammer; }
                        }

                        if(typeToGenerate == ItemType.None)
                        {
                            if (dataRepo.GeneratorData.ItemTypeNumberPairs[ItemType.Coin] > 0)
                                typeToGenerate = ItemType.Coin;
                            if (dataRepo.GeneratorData.ItemTypeNumberPairs[ItemType.BagOfCoin] > 0)
                                typeToGenerate = ItemType.BagOfCoin;
                        }

                        if (dataRepo.GeneratorData.ItemTypeNumberPairs[typeToGenerate] <= 0)
                        {
                            foreach(var item in dataRepo.GeneratorData.ItemTypeNumberPairs)
                            {
                                if (item.Value > 0)
                                {
                                    typeToGenerate = item.Key;
                                    break;
                                }
                                    
                            }
                        }
                        if(dataRepo.GeneratorData.ItemTypeNumberPairs[typeToGenerate] > 0)
                        {
                            dataRepo.GeneratorData.ItemTypeNumberPairs[typeToGenerate]--;
                            InstantiateItemAndAddThemToLists(dataRepo,typeToGenerate);
                        }
                        lastRegularDrop = Time.time;
                    }

                }
            }
            
            yield return null;

        }


    }
    public static void InstantiateItemAndAddThemToLists(DataRepo dataRepo,ItemType itemType)
    {
        Gift go;
        if(itemType== ItemType.Coin)
        {
            go = GameObject.Instantiate(dataRepo.CoinPrefab, dataRepo.GeneratorData.Generator.transform.position, Quaternion.identity);
            dataRepo.CoinList.Add(new GiftTime { Gift = go, Time = Time.time });
            foreach (PlayerData pl in dataRepo.Players)
            {
                pl.ItemsTimeQueue.Enqueue(Time.time);
            }
        }
        if(itemType == ItemType.BagOfCoin)
        {
            go = GameObject.Instantiate(dataRepo.BagOfCoinPrefab, dataRepo.GeneratorData.Generator.transform.position, Quaternion.identity);
            dataRepo.BagOfCoinList.Add(new GiftTime { Gift = go, Time = Time.time });
            foreach (PlayerData pl in dataRepo.Players)
            {
                pl.ItemsTimeQueue.Enqueue(Time.time);
            }
        }
        if (itemType == ItemType.Hammer)
        {
            go = GameObject.Instantiate(dataRepo.HammerPrefab, dataRepo.GeneratorData.Generator.transform.position, Quaternion.identity);
            dataRepo.HammerList.Add(go);
        }

    }
    public static IEnumerator AddCoin(PlayerData playerData, int numToAdd)
    {
        playerData.NumberOfCoin = playerData.NumberOfCoin+numToAdd;
        playerData.NumberOfCoinText.text = playerData.NumberOfCoin.ToString();
        if(numToAdd == 5)
        {
            playerData.Player.starIconNearText.gameObject.SetActive(true);
        }
        playerData.Player.playerScoreText.text = $"+{numToAdd}";

        playerData.Player.playerScoreText.gameObject.SetActive(true);
        yield return new WaitForSeconds(1);
        playerData.Player.playerScoreText.gameObject.SetActive(false);
        playerData.Player.starIconNearText.gameObject.SetActive(false);
    }
    public static IEnumerator FreezePlayer(PlayerData playerData)
    {
 
        if (playerData.Player.IsMainPlayer)
        {
            Debug.Log($"IsFreezFirst:{playerData.IsFrozen}");
        }
        playerData.IsFrozen = true;
        float EndTimer = Time.time + 3;
        float interval = Time.time + 0.1f;

        while (true)
        {
            if (Time.time > EndTimer)
            {
                playerData.Player.Visual.gameObject.SetActive(false);
                break;
            }
            if (Time.time > interval)
            {
                playerData.Player.Visual.gameObject.SetActive(!playerData.Player.Visual.gameObject.activeSelf);
                interval = Time.time + 0.1f;
            }
            yield return null;
        }
        playerData.Player.Visual.gameObject.SetActive(true);
        playerData.IsFrozen = false;
        if (playerData.Player.IsMainPlayer)
        {
            Debug.Log($"IsFreezEnd:{playerData.IsFrozen}");
        }
    }
    public static IEnumerator RotateGenerator(DataRepo dataRepo)
    {
        float NextRadiusIncreasingCheck = Time.time;
        float NextDirectionCheck = Time.time;
        float NextTargetRadius = Time.time;
        float targetRadius = dataRepo.GeneratorData.maxRadius;
        int direction = 1;
        Vector3 centerPosition = dataRepo.GeneratorData.Generator.transform.position; // Store the initial position as the center
        while (true)
        {
            if (dataRepo.ShouldStopGame) yield break;
            if (dataRepo.RemainingTimeInGame < 20 && dataRepo.RemainingTimeInGame > 10)
            {
                dataRepo.GeneratorData.rotationSpeed = 7;
            }
            if (dataRepo.RemainingTimeInGame <= 10)
            {
                dataRepo.GeneratorData.rotationSpeed = 3;
            }
            if (NextRadiusIncreasingCheck < Time.time)
            {
                NextRadiusIncreasingCheck = Time.time + 4;
                int r = Random.Range(0, 2);
                if (r == 0) { dataRepo.GeneratorData.shouldChangeRadius = false; }
                if (r == 1) { dataRepo.GeneratorData.shouldChangeRadius = true; }
                if (dataRepo.RemainingTimeInGame < 20 && dataRepo.RemainingTimeInGame > 10)
                {
                    dataRepo.GeneratorData.shouldChangeRadius = true;
                }
            }
            if (NextDirectionCheck < Time.time)
            {
                NextDirectionCheck = Time.time + 2;
                int r = Random.Range(0, 2);
                if (r == 1) { direction = -1 * direction; }
                else if (dataRepo.RemainingTimeInGame < 20 && dataRepo.RemainingTimeInGame > 10)
                {
                    direction = -1 * direction;
                }
            }

            if (NextTargetRadius < Time.time) 
            {
                NextTargetRadius = Time.time + 6;
                float r = Random.Range(dataRepo.GeneratorData.minRadius, dataRepo.GeneratorData.maxRadius);
                PlayerData mp = null;
                foreach (PlayerData p in dataRepo.Players)
                {
                    if (p.Player.IsMainPlayer)
                    {
                        mp = p;
                    }
                }
                targetRadius = r;
                if (targetRadius > dataRepo.GeneratorData.maxRadius)
                {
                    targetRadius = dataRepo.GeneratorData.maxRadius;
                }
                if(targetRadius< dataRepo.GeneratorData.minRadius)
                {
                    dataRepo.GeneratorData.radiusGrowth = 4;
                }
                else
                {
                    dataRepo.GeneratorData.radiusGrowth = 1;
                }
                //Debug.Log($"TR:{targetRadius}");
            }
            dataRepo.GeneratorData.angle += direction*dataRepo.GeneratorData.rotationSpeed * Time.deltaTime;
            // Increase the angle over time
            if (dataRepo.GeneratorData.shouldChangeRadius) 
            {
                if(dataRepo.GeneratorData.currentRadius<=targetRadius) 
                {
                    // Increase the radius over time
                    dataRepo.GeneratorData.currentRadius += dataRepo.GeneratorData.radiusGrowth * Time.deltaTime;
                }
                if ( targetRadius<= dataRepo.GeneratorData.currentRadius) 
                {
                    dataRepo.GeneratorData.currentRadius -= dataRepo.GeneratorData.radiusGrowth * Time.deltaTime;

                }
            }
            // Calculate the new position relative to the center position
            float x = Mathf.Cos(dataRepo.GeneratorData.angle) * dataRepo.GeneratorData.currentRadius;
            float z = Mathf.Sin(dataRepo.GeneratorData.angle) * dataRepo.GeneratorData.currentRadius;

            // Apply the new position to the GameObject
            dataRepo.GeneratorData.Generator.transform.position = new Vector3(
                centerPosition.x + x,
                dataRepo.GeneratorData.Generator.transform.position.y, // Maintain the current Y position
                centerPosition.z + z
            );


            yield return null;
        }

        
    
}


    
    public static IEnumerator StartRobot(PlayerData playerData, DataRepo dataRepo)
    {
        while (true)
        {
            if (dataRepo.ShouldStopGame) yield break;
            if (playerData.ItemsTimeQueue.Count > 0)
            {
                if (Time.time - playerData.ItemsTimeQueue.Peek() > playerData.SecondToMakeDecision)
                {
                    float d = playerData.ItemsTimeQueue.Dequeue();

                    List<GiftTime> bagList = dataRepo.BagOfCoinList.Where(x => x.Time <= d).ToList();

                    List<GiftTime> coinList = dataRepo.CoinList.Where(x => x.Time <= d).ToList();

                    Dictionary<Gift, float> bagDistances = new Dictionary<Gift, float>();
                    Dictionary<Gift, float> coinDistances = new Dictionary<Gift, float>();

                    foreach (GiftTime g in coinList)
                    {
                        Vector2 cylinderCenterXZ = new Vector2(dataRepo.GroundCenter.x, dataRepo.GroundCenter.z);

                        // Calculate distance from the center of the cylinder
                        float distanceFromCenter = Vector2.Distance(new Vector2( g.Gift.transform.position.x,g.Gift.transform.position.z), cylinderCenterXZ);

                        if (distanceFromCenter < dataRepo.GroundRadius+0.1)
                        {
                            coinDistances.Add(g.Gift, Vector3.Distance(playerData.Player.transform.position, g.Gift.transform.position));
                        }

                    }
                    Gift closestCoin = coinDistances.OrderBy(kv => kv.Value).FirstOrDefault().Key;

                    foreach (GiftTime g in bagList)
                    {
                        Vector2 cylinderCenterXZ = new Vector2(dataRepo.GroundCenter.x, dataRepo.GroundCenter.z);

                        // Calculate distance from the center of the cylinder
                        float distanceFromCenter = Vector2.Distance(new Vector2(g.Gift.transform.position.x, g.Gift.transform.position.z), cylinderCenterXZ);

                        if (distanceFromCenter < dataRepo.GroundRadius+0.1f)
                        {
                            bagDistances.Add(g.Gift, Vector3.Distance(playerData.Player.transform.position, g.Gift.transform.position));
                        }
                    }
                    Gift closestBag = bagDistances.OrderBy(kv => kv.Value).FirstOrDefault().Key;

                    IReadOnlyList<Gift> giftPriorityList = null;
                    {
                        List<Gift> temp = new List<Gift>();
                        switch (playerData.GiftPriority)
                        {
                            case ItemType.BagOfCoin:
                                temp.Add(closestBag);
                                temp.Add(closestCoin);
                                break;
                            case ItemType.Coin:
                                temp.Add(closestCoin);
                                break;
                            case ItemType.None:
                                bool isBagOfCoinFirst = Random.Range(0, 2)==0;
                                if (isBagOfCoinFirst)
                                {
                                    temp.Add(closestBag);
                                    temp.Add(closestCoin);
                                }
                                else
                                {
                                    temp.Add(closestCoin);
                                    temp.Add(closestBag);
                                }
                                break;
                        }
                        giftPriorityList = temp;
                    }

                    for (int i = 0; i < giftPriorityList.Count; ++i)
                    {
                        Gift g = giftPriorityList[i];
                        List<Gift> allItemsRightNow = new List<Gift>();
                        allItemsRightNow.AddRange(dataRepo.HammerList);
                        if (playerData.Player == dataRepo.Players[1].Player)
                        {
                            Debug.Log($"P1 GiftPriorityLoop");
                        }
                        foreach (GiftTime giftTime in dataRepo.CoinList)
                        {
                            allItemsRightNow.Add(giftTime.Gift);

                        }
                        foreach (GiftTime giftTime in dataRepo.BagOfCoinList)
                        {
                            allItemsRightNow.Add(giftTime.Gift);

                        }
                        if (allItemsRightNow.Contains(g))
                        {
                            playerData.TargetItem = g.transform;
                            if (playerData.Player == dataRepo.Players[1].Player)
                            {
                                Debug.Log($"P1 TargetItem in StarRo:{playerData.TargetItem}");
                            }
                            break;
                        }
                    }

                }
                if (!playerData.TargetAvoid)
                {
                    if (dataRepo.HammerList.Count != 0)
                    {
                        Dictionary<Gift, float> hammerDistances = new Dictionary<Gift, float>();

                        foreach (Gift g in dataRepo.HammerList)
                        {
                            hammerDistances.Add(g, Vector3.Distance(playerData.Player.transform.position, g.transform.position));
                        }
                        Gift closestHammer = hammerDistances.OrderBy(kv => kv.Value).FirstOrDefault().Key;
                        if (Vector3.Distance(closestHammer.transform.position, playerData.Player.transform.position) < 2f)
                        {
                            float chance = Random.Range(0f, 100f);

                            if (chance < playerData.ChanceOfAvoidance)
                            {
                                playerData.TargetAvoid = closestHammer.transform;

                            }
                        }
                    }
                }
            }
            if (playerData.TargetItem)
            {
                Vector3 targetCoinPosOnGround =
                    new Vector3(playerData.TargetItem.position.x, dataRepo.GroundCollider.transform.position.y, playerData.TargetItem.position.z);
               
                if (playerData.IsGrounded
                    && 0.4f + playerData.LastJump < Time.time
                    && Vector3.Distance(targetCoinPosOnGround, playerData.Player.transform.position) < 1.3f)
                {
                    playerData.LastJump = Time.time;
                    OnJumpClicked(dataRepo, playerData);
                }
            }
            if (playerData.TargetAvoid)
            {
                if (Vector3.Distance(playerData.TargetAvoid.position, playerData.Player.transform.position) > 1.5f)
                {
                    playerData.TargetAvoid = null;
                }
            }

            if (!playerData.TargetAvoid && !playerData.TargetItem)
            {
                if (playerData.TargetMovement == Vector3.zero)
                {
                    playerData.TargetMovement = GetRandomPositionOnGround(dataRepo,playerData);

                }
                if (Vector3.Distance(playerData.TargetMovement, playerData.Player.transform.position) < 0.1f)
                {
                    playerData.TargetMovement = Vector3.zero;
                }
                //playerData.PlayerAnimator.SetFloat("MoveSpeed", 0);
            }
            else
            {
                playerData.TargetMovement = Vector3.zero;
            }

            yield return null;
        }
    }



    public static void RemoveGiftFromLists(DataRepo dataRepo, Gift giftShouldBeRemoved) 
    {
        foreach (PlayerData p in dataRepo.Players)
        {

            if (p.TargetItem == giftShouldBeRemoved.transform)
            {
                p.TargetItem = null;
                if (p.Player == dataRepo.Players[1].Player)
                {
                    Debug.Log($"P1 TargetItem in RemoveFro:{p.TargetItem}");
                }
            }
            if (p.TargetAvoid == giftShouldBeRemoved.transform)
            {
                p.TargetAvoid = null;
            }
        }
        for (int i = 0; i < dataRepo.HammerList.Count;i++)
        {
            if (dataRepo.HammerList[i] == giftShouldBeRemoved)
            {
                dataRepo.HammerList.RemoveAt(i);
                return;
            }
        }
        for (int i = 0; i < dataRepo.CoinList.Count; i++)
        {
            if (dataRepo.CoinList[i].Gift == giftShouldBeRemoved)
            {
                dataRepo.CoinList.RemoveAt(i);
                return;
            }
        }
        for (int i = 0; i < dataRepo.BagOfCoinList.Count; i++)
        {
            if (dataRepo.BagOfCoinList[i].Gift == giftShouldBeRemoved)
            {
                dataRepo.BagOfCoinList.RemoveAt(i);
                return;
            }
        }
    }
    public static Vector3 GetRandomPositionOnGround(DataRepo dataRepo, PlayerData playerData)
    {
        // Calculate the effective radius of the ground
        float radius = dataRepo.GroundCollider.localScale.x * 0.3f;

        // Generate a random angle in radians
        float angle = Random.Range(0f, Mathf.PI * 2);

        // Calculate x and z coordinates relative to the center of the ground
        float x = Mathf.Cos(angle) * radius;
        float z = Mathf.Sin(angle) * radius;

        // Adjust the position to be relative to the ground's actual position
        float adjustedY = dataRepo.GroundCollider.position.y + (dataRepo.GroundCollider.localScale.y); // Top surface of the ground

        return new Vector3(
            x + dataRepo.GroundCollider.position.x, // Adjust x based on the ground's center position
            adjustedY, // Set Y to the ground's top surface
            z + dataRepo.GroundCollider.position.z  // Adjust z based on the ground's center position
        );
    }

}



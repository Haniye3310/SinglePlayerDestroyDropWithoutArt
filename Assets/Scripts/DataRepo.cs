using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DataRepo : MonoBehaviour
{
    public ConfigData ConfigData;
    public FixedJoystick Joystick;
    public List<PlayerData> Players;
    public List<GameObject> BotsPrefabs;
    public Gift CoinPrefab;
    public Gift BagOfCoinPrefab;
    public Gift HammerPrefab;

    [NonSerialized] public int TimeOftheGame = 30;
    [NonSerialized] public float RemainingTimeInGame;
    [NonSerialized] public bool ShouldStopGame = false;

    public TextMeshProUGUI RemainingTimeText;
    public Image StartCountDownTimer;
    public Image GoImage;
    public Sprite NumberThreeSprite;
    public Sprite NumberTwoSprite;
    public Sprite NumberOneSprite;
    public GameObject UIPanel;
    public Image TimerImage;
    public GameObject ResultPanel;
    public Sprite FirstSprite;
    public Sprite SecondSprite;
    public Sprite ThirdSprite;
    public Sprite FourthSprite;
    public Image ResultPanelBG;

    [NonSerialized] public bool ShouldDropPrizeNearPlayer;
    [NonSerialized] public bool ShouldDropPrizeNearBots;
    [NonSerialized] public float DropRegularThreshold = 0.95f;



    [NonSerialized]public List<Gift> HammerList = new List<Gift>();
    [NonSerialized]public List<GiftTime> CoinList = new List<GiftTime>();
    [NonSerialized]public List<GiftTime> BagOfCoinList = new List<GiftTime>();

    [NonSerialized] public Vector3 GroundCenter;
    [NonSerialized] public float GroundRadius;
    public Transform GroundCollider;

    public GeneratorData GeneratorData;


}
[Serializable]
public class PlayerData 
{
    public Player Player;
    public BotLevel BotLevel;
    public Animator PlayerAnimator;
    public Rigidbody PlayerRigidbody;
    public float Strength;
    [NonSerialized]public float LastJump;

    
    [NonSerialized] public int NumberOfCoin;
    public TextMeshProUGUI NumberOfCoinText;
    [NonSerialized]public bool IsFrozen = false; // To track if the player is frozen
    [NonSerialized] public bool IsGrounded =true;
    [NonSerialized] public bool ShouldJump;
    [NonSerialized] public Vector3 PushForce;
    [NonSerialized] public Transform TargetAvoid;
    [NonSerialized] public Transform TargetItem;
    [NonSerialized] public Vector3 TargetMovement;
    [NonSerialized] public Queue<float> ItemsTimeQueue = new Queue<float>();
    [Header("BotVariables")]
    [SerializeField]public float SecondToMakeDecision;
    [SerializeField] public int ChanceOfAvoidance;
    [SerializeField] public ItemType GiftPriority;
}
public enum BotLevel
{
    Good,Bad,Medium,Player
}
public class GiftTime
{
    public Gift Gift;
    public float Time;
}
[Serializable]
public class GeneratorData 
{
    [NonSerialized]public float radiusGrowth = 1f;  // How much the radius grows per second
    [NonSerialized]public float rotationSpeed = 3f;  // Speed of rotation around the center point
    [NonSerialized]public float angle = 0f;
    [NonSerialized]public float currentRadius = 0.3f;
    public GameObject Generator;
    [NonSerialized]public int MaxNumberOfRotation =3;
    [NonSerialized]public bool shouldChangeRadius;
    [NonSerialized]public float maxRadius;
    [NonSerialized]public float minRadius ;

    [Header("GenerationDifficulty")]
    [NonSerialized] public Dictionary<ItemType, int> ItemTypeNumberPairs = new Dictionary<ItemType, int>();
}
[Serializable]
public class ConfigData
{
    public float SpeedOfCharacterMovement;
    public float JumpSpeed;
    [NonSerialized]public float JumpVelocity = 5;
    [NonSerialized]public float FallMultiplier = 2.5f;
    [NonSerialized]public float lowJumpMultiplier = 2f;
}
public enum ItemType
{
    Coin,
    BagOfCoin,
    Hammer,
    /// <summary>
    /// Choose randomly between Coin or BagOfCoin
    /// </summary>
    None
}
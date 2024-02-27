using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour, IOverlayManager
{
    private static GameManager _instance;
    [SerializeField] private Die[] _dice;
    [SerializeField] private GameObject[] _messager;
    [SerializeField] public PlayerInfoBox PlayerInfoBox;
    [SerializeField] private TMP_Text _potMoney;
    [SerializeField] public Button FoldButton;
    [SerializeField] public Button BetOrRaiseButton;
    [SerializeField] public Button CheckOrCallButton;
    [SerializeField] public Button RollDiceButton;
    [SerializeField] public Button EndButton;
    [SerializeField] public GameObject DiceHint;
    [SerializeField] public Slider BettingSlider;
    [SerializeField] private TMP_Text _chat;
    [SerializeField] private TMP_InputField _chatInput;
    [SerializeField] public TMP_Text BettingValue;
    [SerializeField] public WinScreen WinScreen;
    [SerializeField] public DiceThrow DiceThrow;

    private readonly List<Message> _messages = new();
    private const int FRAMES = 300;

    private string[] _playerNames;

    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("GameManager is null");
            }
            return _instance;
        }
    }
    void Awake()
    {
        _instance = this;
        GameHandler.Instance.OverlayManager = this;
        WinScreen.gameObject.SetActive(false);
    }

    void Start()
    {
        //SetDiceActive(false);
        StartCoroutine(MessageCoroutine());
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && DiceHint.activeSelf)
        {
            GameHandler.Instance.client.PlayerRollDice();
            DisableButtons();
        }
    }

    public void UpdateSliderValue()
    {
        SetBettingSliderValue((int) BettingSlider.value);
    }

    public void AddMessage(int type, string msg)
    {
        foreach (var lmsg in _messages)
        {
            if (lmsg.type == type && lmsg.msg == msg)
            {
                return;
            }
        }
        Message message;
        message.type = type;
        message.msg = msg;
        _messages.Add(message);
    }

    IEnumerator MessageCoroutine()
    {
        bool active = false;
        while (true)
        {
            if (_messages.Count > 0 && !active)
            {
                active = true;
                Message msg = _messages[0];

                var msger = _messager[msg.type];
                msger.GetComponent<Image>().GetComponentInChildren<TMP_Text>().text = msg.msg;
                msger.SetActive(true);
                yield return new WaitForSeconds(2);
                msger.SetActive(false);
                active = false;
                _messages.RemoveAt(0);
            }
            else
            {
                yield return null;
            }
        }
    }

    public void PlayerRollDice()
    {
        GameHandler.Instance.client.PlayerRollDice();
        DisableButtons();
    }
    public void PlayerBetOrRaise()
    {
        GameHandler.Instance.client.PlayerBetOrRaise((int) BettingSlider.value);
        DisableButtons();
    }

    public void PlayerCheckOrCall()
    {
        GameHandler.Instance.client.PlayerCheckOrCall();
        DisableButtons();
    }

    public void PlayerFold()
    {
        GameHandler.Instance.client.PlayerFold();
        DisableButtons();
    }

    public void PlayerEndRoll()
    {
        GameHandler.Instance.client.PlayerEndDiceRolls();
        DisableButtons();
    }

    public void SetDiceActive(bool b)
    {
        foreach (var die in _dice)
        {
            die.gameObject.SetActive(b);
        }
    }

    public void SetupGame(PokerGame game, string[] playerNames, bool money)
    {
        _playerNames = playerNames;
        for (int i = 0; i < playerNames.Length; i++)
        {
            PlayerInfoBox.AddInfo(playerNames[i], game.State.players[i], money);
        }
        SetPotMoney(0);
    }

    public void DisableButtons()
    {
        FoldButton.interactable = false;
        BetOrRaiseButton.interactable = false;
        CheckOrCallButton.interactable = false;
        RollDiceButton.interactable = false;
        EndButton.interactable = false;
        BettingSlider.interactable = false;
        DiceHint.SetActive(false);       
    }

    public void EnableRoundButtons(int min, int max)
    {
        BetOrRaiseButton.interactable = true;
        BettingSlider.interactable = true;
        BettingSlider.minValue = min;
        BettingSlider.maxValue = max;
        CheckOrCallButton.interactable = true;
        FoldButton.interactable = true;
    }

    public void SetPotMoney(int money)
    {
        _potMoney.text = "Pot: " + money + "$";
    }

    public void ClearPot()
    {
        _potMoney.text = "";
    }

    public void TestThrow()
    {
        ThrowDice(new List<int>() { 5, 5, 5, 5, 5 }, new bool[] {true, true, true, true, true});
    }

    public void ThrowDice(List<int> numbers, bool[] selection)
    {
        List<Die> dice = new(5);
        List<int> diceNumbers = new(5);
        for (int i = 0; i < _dice.Length; i++)
        {
            if (!selection[i])
            {
                continue;
            }
            dice.Add(_dice[i]);
            diceNumbers.Add(numbers[i]);
        }
        DiceThrow.ShowDice(numbers);
        SimulatePhysics(diceNumbers, dice);
    }

    public void DiceLanded()
    {
        bool b = true;
        foreach (var die in _dice)
        {
            if (!die.CheckObjectStoppedMoving())
            {
                b = false;
                break;
            }
        }
        if (b)
        {

        }
    }

    public void SendChatMessage()
    {
        GameHandler.Instance.client.SendChatMessage(_chatInput.text);
        _chatInput.text = "";
    }

    public void AddChatMesage(int player, string msg)
    {
        _chat.text += _playerNames[player] + ": " + msg + "\n";
    }

    public void SimulatePhysics(List<int> numbers, List<Die> dice)
    {
        foreach (var die in dice)
        {
            Debug.Log("throw");
            die.SetInitialState();
            die.InitRecorder(FRAMES);
            //die.EnablePhysics();
        }
        Physics.simulationMode = SimulationMode.Script;
        for (int i = 0; i < FRAMES; i++)
        {
            Physics.Simulate(Time.fixedDeltaTime);
            foreach (var die in dice)
            {
                die.RecordFrame(i);
            }
        }
        Physics.simulationMode = SimulationMode.FixedUpdate;

        for (int i = 0; i < dice.Count; i++)
        {
            var die = dice[i];
            int n = die.GetNumber();
            Debug.Log(n);
            //die.DisablePhysics();
            die.RevertToState();
            die.RotateTo(n, numbers[i]);
        }
        StartCoroutine(PlayAnimation(dice));
    }

    private IEnumerator PlayAnimation(List<Die> dice)
    {
        for (int i = 0; i < FRAMES; i++)
        {
            foreach (var die in dice)
            {
                die.SetFrame(i);
            }
            yield return new WaitForFixedUpdate();
        }
    }
    public void SetBettingSliderValue(int value)
    {
        BettingValue.text = value + "";
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    public Die[] Dice => _dice;

}

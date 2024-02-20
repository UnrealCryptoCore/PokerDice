using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour, IOverlayManager
{
    private static GameManager _instance;
    [SerializeField] private Die[] _dice;
    private Vector3[] _dicePos;
    [SerializeField] private GameObject[] _messager;
    [SerializeField] public PlayerInfoBox PlayerInfoBox;
    [SerializeField] private TMP_Text _potMoney;
    [SerializeField] public Button FoldButton;
    [SerializeField] public Button BetOrRaiseButton;
    [SerializeField] public Button CheckOrCallButton;
    [SerializeField] public Button RollDiceButton;
    [SerializeField] public Button EndButton;

    private readonly List<Message> _messages = new();
    private const int FRAMES = 300;

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
        GameHandler.Instance.overlayManager = this;
    }

    void Start()
    {
        StartCoroutine(MessageCoroutine());
    }

    // Update is called once per frame
    void Update()
    {

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
        GameHandler.Instance.client.PlayerBetOrRaise(50);
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

    public void SetupGame(PokerGame game, string[] playerNames, bool money)
    {
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
    }

    public void EnableRoundButtons()
    {
        BetOrRaiseButton.interactable = true;
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
        List<int> diceNumbers = new();
        for (int i = 0; i < _dice.Length; i++)
        {
            if (!selection[i])
            {
                continue;
            }
            dice.Add(_dice[i]);
            diceNumbers.Add(numbers[i]);
        }
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

    public Die[] Dice => _dice;

}

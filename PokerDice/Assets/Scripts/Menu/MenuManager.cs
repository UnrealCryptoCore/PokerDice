using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour, IOverlayManager
{

    private static MenuManager _instance;
    private static bool _initialzed = false;
    public TMP_InputField UsernameField;
    public TMP_InputField AddressField;
    public TMP_InputField GameIdField;
    public GameObject UsernameInputPanel;
    public GameObject[] Messager;
    public GameObject[] Menus;

    [SerializeField] private Button _createGameButton;
    [SerializeField] private TMP_InputField _moneyField;
    [SerializeField] private TMP_InputField _minimumField;
    [SerializeField] private TMP_InputField _maximumField;
    [SerializeField] private TMP_InputField _maxRoundsField;
    [SerializeField] private Toggle _bettingField;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private Slider _audioSlider;
    [SerializeField] private Toggle _audioToggle;

    public List<Message> Messages = new();

    public static MenuManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("MenuManager is null");
            }
            return _instance;
        }
    }
    void Awake()
    {

        _instance = this;
        GameHandler.Instance.OverlayManager = this;
    }

    void Start()
    {
        if (!_initialzed)
        {
            string username = PlayerPrefs.GetString("username");
            if (username == "")
            {
                username = "Player" + UnityEngine.Random.Range(0, 10000);
                PlayerPrefs.SetString("user", username);
            }
            string address = PlayerPrefs.GetString("address");
            if (address == "")
            {
                address = "127.0.0.1";
                PlayerPrefs.SetString("address", address);
            }
            UsernameField.text = username;
            AddressField.text = address;
            if (!PlayerPrefs.HasKey("volume"))
            {
                PlayerPrefs.SetFloat("volume", 0.1f);
                UpdateAudio();
            }
            float volume = PlayerPrefs.GetFloat("volume");
            _audioSlider.value = volume;
            if (!PlayerPrefs.HasKey("audio"))
            {
                PlayerPrefs.SetInt("audio", 1);
                SetAudioActive();
            }
            ConnectToServer(username, address);
            int audio = PlayerPrefs.GetInt("audio");
            _audioToggle.isOn = audio == 1;
            UsernameField.onEndEdit.AddListener(OnUsernameEdit);
            AddressField.onEndEdit.AddListener(OnAddressEdit);
            StartCoroutine(MessageCoroutine());
            _initialzed = true;
        }
    }

    private void ConnectToServer(string username, string ip)
    {
        try
        {
            GameHandler.Instance.client.StartConnection(ip);
            GameHandler.Instance.client.RegisterName(username);
        }
        catch (SocketException)
        {
            AddMessage(0, "Could not connect to server!");
        }
        catch (FormatException)
        {
            AddMessage(0, "Could not connect to server!");
        }
    }

    void Update()
    {
    }

    public void UpdateCreateButton()
    {
        _createGameButton.interactable = VerifiyInput();
    }

    public bool VerifiyInput()
    {
        if (!int.TryParse(_moneyField.text, out int money) || money < 1)
        {
            return false;
        }
        if (!int.TryParse(_minimumField.text, out int minimum) || minimum < 1)
        {
            return false;
        }
        if (!int.TryParse(_maximumField.text, out int maximum) || maximum < 1)
        {
            return false;
        }

        if (!int.TryParse(_maxRoundsField.text, out int maxRounds) || maxRounds < 1)
        {
            return false;
        }
        return true;
    }

    public void OnAddressEdit(string address)
    {
        ConnectToServer(PlayerPrefs.GetString("username"), address);
        PlayerPrefs.SetString("address", address);
    }
    public void OnUsernameEdit(string name)
    {
        PlayerPrefs.SetString("username", name);
        if (!GameHandler.Instance.client.Connected)
        {
            AddMessage(0, "No connection to server!");
            return;
        }
        GameHandler.Instance.client.RegisterName(name);
    }

    public void RequestCreateGame()
    {
        if (!GameHandler.Instance.client.Connected)
        {
            AddMessage(0, "No connection to server!");
            return;
        }
        var settings = new GameSettings
        {
            money = int.Parse(_moneyField.text),
            minimum = int.Parse(_minimumField.text),
            maximum = int.Parse(_maximumField.text),
            maxRounds = int.Parse(_maxRoundsField.text),
            betting = _bettingField.isOn,
            allowRaisingOnce = true,
        };
        GameHandler.Instance.client.RequestCreateGame(settings);
    }

    public void RequestJoinGame()
    {
        if (!GameHandler.Instance.client.Connected)
        {
            AddMessage(0, "No connection to server!");
            return;
        }
        string gameid = GameIdField.text;
        if (gameid.Length < 2)
        {
            AddMessage(0, "Invalid game id");
        }
        GameHandler.Instance.client.RequestJoinGame(gameid);
    }

    public void RequestGameStart()
    {
        if (!GameHandler.Instance.client.Connected)
        {
            AddMessage(0, "No connection to server!");
            return;
        }
        GameHandler.Instance.client.RequestGameStart();

    }
    IEnumerator MessageCoroutine()
    {
        bool active = false;
        while (true)
        {
            if (Messages.Count > 0 && !active)
            {
                active = true;
                Message msg = Messages[0];

                var msger = Messager[msg.type];
                msger.GetComponent<Image>().GetComponentInChildren<TMP_Text>().text = msg.msg;
                msger.SetActive(true);
                yield return new WaitForSeconds(2);
                msger.SetActive(false);
                active = false;
                Messages.RemoveAt(0);
            }
            else
            {
                yield return null;
            }
        }
    }

    public void AddMessage(int type, string msg)
    {
        // check for duplicate messages
        foreach (var lmsg in Messages)
        {
            if (lmsg.type == type && lmsg.msg == msg)
            {
                return;
            }
        }
        Message message;
        message.type = type;
        message.msg = msg;
        Messages.Add(message);
    }

    public void OpenMenu(int idx)
    {
        if (GameHandler.Instance.client.InGame)
        {
            if (idx == (int)Menu.CREATE_GAME)
            {
                idx = 2;
            }
            else if (idx == (int)Menu.PLAY_GAME)
            {
                return;
            }

        }

        foreach (var menu in Menus)
        {
            menu.SetActive(false);
        }
        Menus[idx].SetActive(true);
    }

    public void QuitApplication()
    {
        Application.Quit();
    }

    public void UpdateAudio()
    {
        PlayerPrefs.SetFloat("volume", _audioSlider.value);
        GameHandler.Instance.SetSoundVolume(_audioSlider.value);
    }

    public void SetAudioActive()
    {
        PlayerPrefs.SetInt("audio", _audioToggle.isOn ? 1 : 0);
        GameHandler.Instance.SetAudioActive(_audioToggle.isOn);
    }
}

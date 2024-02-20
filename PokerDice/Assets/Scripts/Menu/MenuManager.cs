using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Web;
using System.Runtime.Serialization;

public class MenuManager : MonoBehaviour, IOverlayManager
{


    private static MenuManager _instance;
    public TMP_InputField UsernameField;
    public TMP_InputField GameIdField;
    public GameObject UsernameInputPanel;
    public GameObject[] Messager;
    public GameObject[] Menus;

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
        GameHandler.Instance.overlayManager = this;
    }

    void Start()
    {
        string username = PlayerPrefs.GetString("username");
        if (username == "")
        {
            username = "Player" + Random.Range(0, 10000);
            PlayerPrefs.SetString("username", username);
        }
        UsernameField.text = username;
        try
        {
            GameHandler.Instance.client.StartConnection();
            GameHandler.Instance.client.RegisterName(username);
        }
        catch (SocketException e)
        {
            AddMessage(0, "Could not connect to server!");
            Debug.LogWarning(e);
        }
        UsernameField.onEndEdit.AddListener(OnUsernameEdit);
        StartCoroutine(MessageCoroutine());
    }

    void Update()
    {
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

        GameHandler.Instance.client.RequestCreateGame(new GameSettings());
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
}

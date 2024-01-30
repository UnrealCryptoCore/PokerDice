using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class PDNetworkManager : MonoBehaviour
{
    private static PDNetworkManager instance;
    public const string SPLITTER = "#";
    private TcpClient _client;
    private NetworkStream _stream;
    private IPEndPoint _ip;
    private bool _running;
    private Dictionary<string, Func<string, Packet>> _packetHandlers;

    public static PDNetworkManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject singleton = new("NetworkManager");
                instance = singleton.AddComponent<PDNetworkManager>();
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            _client = new();
            _ip = new(IPAddress.Parse("127.0.0.1"), 12345);
            _running = false;
            _packetHandlers = new();
            DontDestroyOnLoad(gameObject);
        }
    }

    public void StartConnection()
    {
        _client.Connect(_ip);
        _stream = _client.GetStream();
        _running = true;

        Task.Run(() => HandleNetworkStream());

        /*new Thread(() =>
        {
            while (_running)
            {
                Debug.Log("listening:..");

            }

        }).Start();*/
    }

    private async Task HandleNetworkStream()
    {
        string msgData = "";
        while (_running)
            {
                Debug.Log("listening:..");
                byte[] data = new byte[1024];
                int bytes = await _stream.ReadAsync(data, 0, data.Length);
                msgData += System.Text.Encoding.UTF8.GetString(data);
                Debug.Log(msgData);
                string[] msgs = msgData.Split(SPLITTER);
                if (bytes == 0 || msgs.Length == 0)
                {
                    continue;
                }
                if (!msgData.EndsWith(SPLITTER)) // checking if last packet is completly here else wait for the next bytes
                {
                    msgData = msgs[^1];
                }
                foreach (var msg in msgs)
                {
                    if (msg == "") {
                        continue;
                    }
                    Debug.Log(msg);
                    HandleMessage(msg);
                }
            }
    }
    
    private void HandleMessage(string msg)
    {
        Packet packet = JsonUtility.FromJson<Packet>(msg);
        if (packet == null)
        {
            return;
        }
        var handler = _packetHandlers[packet.type];
        if (handler == null)
        {
            return; // packet type not initialized
        }
        Packet res = handler(packet.data);
        if (res == null)
        {
            return;
        }
        string json = JsonUtility.ToJson(new Packet(packet.type, JsonUtility.ToJson(res))) + SPLITTER;
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);
        _stream.Write(bytes);
    }

    public void AddPacketHandler(string type, Func<string, Packet> handler)
    {
        _packetHandlers.Add(type, handler);
    }

    public void SendPacket(string type, object data)
    {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(new Packet(type, JsonUtility.ToJson(data))) + SPLITTER);
        _stream.Write(bytes);
    }
    private void OnApplicationQuit()
    {
        _running = false;
    }
}

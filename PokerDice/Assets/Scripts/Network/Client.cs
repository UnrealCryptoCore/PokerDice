using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class Client
{
    public const string SPLITTER = "#";
    private TcpClient _client;
    private NetworkStream _stream;
    private IPEndPoint _ip;
    private bool _running;
    private Dictionary<string, Func<string, Packet>> _packetHandlers;



    public Client()
    {
        _running = false;
        _packetHandlers = new();
    }

    public void StartConnection(string hostname)
    {
        _client = new();
        bool b = IPAddress.TryParse(hostname, out IPAddress address);
        if (!b)
        {
            IPHostEntry entry = Dns.GetHostEntry(hostname);
            if (entry.AddressList.Length > 0)
            {
                address = entry.AddressList[0];
            }
            else
            {
                throw new SocketException();
            }
        }
        _ip = new(address, 12345);
        _client.Connect(_ip);
        _stream = _client.GetStream();
        _running = true;

        Task.Run(() => HandleNetworkStream());
    }

    private async Task HandleNetworkStream()
    {
        string msgData = "";
        while (_running)
        {
            byte[] data = new byte[1024];
            int bytes = await _stream.ReadAsync(data, 0, data.Length);
            msgData += System.Text.Encoding.UTF8.GetString(data, 0, bytes);
            Debug.Log(msgData);
            string[] msgs = msgData.Split(SPLITTER);
            if (bytes == 0)
            {
                _running = false;
                GameHandler.Instance.Enqueue(() => GameHandler.Instance.OverlayManager.AddMessage(0, "Lost connection to server"));
                //Debug.LogError("Server unreachable");
                continue;
            }
            if (msgs.Length == 0)
            {
                continue;
            }
            bool b = !msgData.EndsWith(SPLITTER);
            if (b) // checking if last packet is completly here else wait for the next bytes
            {
                msgData = msgs[^1];
            }
            else
            {
                msgData = "";
            }
            for (int i = 0; i < (b ? msgs.Length - 1 : msgs.Length); i++)
            {
                if (msgs[i] == "")
                {
                    continue;
                }
                var msg = msgs[i];
                Debug.Log(msg);
                GameHandler.Instance.Enqueue(() => HandleMessage(msg));
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
        string json = JsonUtility.ToJson(new Packet("", JsonUtility.ToJson(res))) + SPLITTER;
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);
        _stream.Write(bytes);
    }

    public void CloseConnection()
    {
        _running = false;
        _client.Close();
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

    public bool Connected => _client != null;
}

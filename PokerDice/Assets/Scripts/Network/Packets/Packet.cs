
using UnityEngine;

[System.Serializable]
public class Packet
{
    public const string USER_REGISTRY_PACKET = "USER_REGISTRY";
    public const string GAME_REQUEST_PACKET = "GAME_REQUEST";
    public const string GAME_CREATE_REQUEST_PACKET = "GAME_CREATE_REQUEST";
    public const string GAME_JOIN_REQUEST_PACKET = "GAME_JOIN_REQUEST";
    public const string GAME_START_REQUEST_PACKET = "GAME_START_REQUEST";
    public const string GAME_JOIN_PACKET = "GAME_JOIN";
    public const string GAME_START_PACKET = "GAME_START";
    public const string USER_REGISTRY_ERROR_PACKET = "USER_REGISTRY_ERROR";
    public const string GAME_REQUEST_ERROR_PACKET = "GAME_REQUEST_ERROR";

    public string type;
    public string data;

    public Packet(string type, string data)
    {
        this.type = type;
        this.data = data;
    }

    /*public byte[] ToBytes(int padding)
    {
        byte[] data = new byte[padding];
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(this));
        for (int i = 0; i < bytes.Length; i++) {
            data[i] = bytes[i];
        }
        for (int i = bytes.Length; i < padding; i++) {
            data[i] = PAD;
        }
        return data;
    }*/
    public static T FromBytes<T>(byte[] bytes)
    {
        return JsonUtility.FromJson<T>(System.Text.Encoding.UTF8.GetString(bytes));
    }
}

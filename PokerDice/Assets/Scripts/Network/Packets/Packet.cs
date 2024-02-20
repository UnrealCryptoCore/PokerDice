
using System;
using UnityEngine;

[Serializable]
public class Packet
{
    public const string USER_REGISTRY_PACKET = "USER_REGISTRY";
    public const string GAME_REQUEST_PACKET = "GAME_REQUEST";
    public const string GAME_CREATE_REQUEST_PACKET = "GAME_CREATE_REQUEST";
    public const string GAME_JOIN_REQUEST_PACKET = "GAME_JOIN_REQUEST";
    public const string GAME_START_REQUEST_PACKET = "GAME_START_REQUEST";
    public const string GAME_FOLD_PACKET = "GAME_FOLD";
    public const string GAME_CHECK_OR_CALL_PACKET = "GAME_CHECK_OR_CALL";
    public const string GAME_BET_OR_RAISE_PACKET = "GAME_BET_OR_RAISE";
    public const string GAME_ROLL_DICE_PACKET = "GAME_ROLL_DICE";
    public const string GAME_END_ROLLS_PACKET = "GAME_END_ROLLS";
    public const string GAME_ANTE_PACKET = "GAME_ANTE";
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

[Serializable]
public class EmptyPacket
{

}

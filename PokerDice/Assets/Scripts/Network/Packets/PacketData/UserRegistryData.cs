using System;

[Serializable]
public class UserRegistryData
{
    public string name;

    public UserRegistryData(string name) {
        this.name = name;
    }
}

[Serializable]
public class UserRegistryPacket
{
    public string token;
}
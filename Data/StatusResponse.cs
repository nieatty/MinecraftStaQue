using System.Net;
using Newtonsoft.Json;

namespace MinecraftStaQue.Data;

[JsonObject(MemberSerialization.OptIn)]
public class StatusVersion
{
    [JsonProperty("name")] public string Name;

    [JsonProperty("protocol")] public int Protocol;
}

[JsonObject(MemberSerialization.OptIn)]
public class StatusPlayer
{
    [JsonProperty("id")] public string? Id;
    [JsonProperty("name")] public string Name;
}

[JsonObject(MemberSerialization.OptIn)]
public class StatusPlayers
{
    [JsonProperty("max")] public int Max;

    [JsonProperty("online")] public int Online;

    [JsonProperty("sample")] public StatusPlayer[]? Sample;
}

[JsonObject(MemberSerialization.OptIn)]
public class StatusDescription
{
    [JsonProperty("text")] public string? Text;
}

[JsonObject(MemberSerialization.OptIn)]
public class StatusResponse
{
    public IPAddress? Address;
    [JsonProperty("description")] public StatusDescription? Description;

    [JsonProperty("favicon")] public string? Favicon;

    [JsonProperty("modinfo")] public string? ModInfo;
    public bool Online;

    [JsonProperty("players")] public StatusPlayers? Players;
    public ushort? Port;

    [JsonProperty("previewsChat")] public bool? PreviewsChat;
    public byte[]? Raw;

    [JsonProperty("version")] public StatusVersion? Version;
}
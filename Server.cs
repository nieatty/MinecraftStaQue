using System.Net;
using System.Net.Sockets;
using MinecraftStaQue.Data;
using Newtonsoft.Json;

namespace MinecraftStaQue;

public class Server
{
    private const int Timeout = 3000;
    private const int BufferLength = 64 * 1024; // 64KB

    private TcpClient? _connection;

    public Server(IPAddress ip, ushort port)
    {
        Ip = ip;
        Port = port;
    }

    public IPAddress Ip { get; }
    public ushort Port { get; }

    public override string ToString()
    {
        return $"{Ip}:{Port}";
    }

    public StatusResponse GetStatus()
    {
        if (!Connect())
            goto Error;

        var handshake = new Packet(0)
            .WriteVarInt(-1)
            .WriteString(".")
            .WriteShort(0)
            .WriteVarInt(1)
            .ToBytes();

        var status = new Packet(0)
            .ToBytes();

        Send(handshake);
        Send(status);

        var response = Receive();

        Disconnect();

        if (response == null)
            goto Error;

        var packet = Packet.FromBytes(response);
        if (packet.Id != 0)
            goto Error;

        var jsonRaw = packet.ReadString();
        var jsonResponse = JsonConvert.DeserializeObject<StatusResponse>(jsonRaw);

        if (jsonResponse == null)
            goto Error;

        jsonResponse.Address = Ip;
        jsonResponse.Port = Port;
        
        return jsonResponse;

        Error:
        return new StatusResponse
        {
            Online = false
        };
    }

    private bool Connect()
    {
        if (IsConnected())
            return true;

        try
        {
            _connection = new TcpClient();
            _connection.SendTimeout = Timeout;
            _connection.Connect(Ip, Port);
            return IsConnected();
        }
        catch (Exception)
        {
            return false;
        }
    }

    private void Disconnect()
    {
        _connection?.Close();
        _connection = null;
    }

    private bool IsConnected()
    {
        return _connection is { Connected: true };
    }

    private void Send(byte[] data)
    {
        if (!IsConnected())
            return;

        _connection?.GetStream().Write(data, 0, data.Length);
    }

    private byte[]? Receive()
    {
        if (!IsConnected())
            return null;

        var stream = _connection?.GetStream();
        var buffer = new byte[BufferLength];
        _ = stream?.Read(buffer, 0, buffer.Length);
        return buffer;
    }
}
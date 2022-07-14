using System.Net;
using System.Net.Sockets;
using MinecraftSnQ.Data;
using Newtonsoft.Json;

namespace MinecraftSnQ;

public class Server
{
    private const int Timeout = 3000;
    private const int BufferLength = 64 * 1024; // 64KB

    private TcpClient? _connection;

    public Server(IPAddress ip, ushort port)
    {
        IP = ip;
        Port = port;
    }

    public IPAddress IP { get; }
    public ushort Port { get; }

    public override string ToString()
    {
        return $"{IP}:{Port}";
    }

    public StatusResponse GetStatus()
    {
        if (!Connect())
            return new StatusResponse
            {
                Online = false
            };

        var handshake = new Packet(0)
            .WriteVarInt(0)
            .WriteString(".")
            .WriteShort(0)
            .WriteVarInt(1)
            .ToBytes();

        var status = new Packet(0)
            .ToBytes();

        Send(handshake);
        Send(status);

        var response = Receive();
        if (response == null)
            return new StatusResponse
            {
                Online = false
            };

        var packet = Packet.FromBytes(response);
        if (packet.Id != 0)
            return new StatusResponse
            {
                Online = false
            };

        var JsonResponse = packet.ReadString();
        return JsonConvert.DeserializeObject<StatusResponse>(JsonResponse) ?? new StatusResponse
        {
            Online = false
        };
    }

    private bool Connect()
    {
        if (_connection != null)
            return true;

        try
        {
            _connection = new TcpClient();
            _connection.SendTimeout = Timeout;
            _connection.Connect(IP, Port);
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
namespace MinecraftSnQ.Data;

public class Packet
{
    private readonly List<byte> _data;
    private int _position;

    public Packet(int id)
    {
        _data = new List<byte>();
        Id = id;
        _position = 0;
    }

    public int Id { get; private set; }

    public byte[] ToBytes()
    {
        var buffer = new List<byte>();

        // Create temporary packet
        var p = new Packet(0);
        p.WriteVarInt(Id);

        // Copy id from temporary packet
        buffer.AddRange(p._data);
        p._data.Clear();
        p._position = 0;

        // Copy data from this packet
        buffer.AddRange(_data);

        // Add length of packet to beginning of buffer
        var length = buffer.Count;
        p.WriteVarInt(length);
        p.WriteBytes(_data);

        // Now the temporary packet contains the full packet
        return p._data.ToArray();
    }

    public static Packet FromBytes(byte[] data)
    {
        var p = new Packet(0);
        p._data.AddRange(data);

        // Read size of packet
        var size = p.ReadVarInt();

        // Read id of packet
        var id = p.ReadVarInt();

        // Remove size and id from packet
        p._data.RemoveRange(0, p._position - 1);
        p._position = 0;
        p.Id = id;

        if (size != p._data.Count) throw new Exception("Packet size mismatch");

        return p;
    }

    public Packet WriteByte(byte value)
    {
        _data.Add(value);
        _position++;

        return this;
    }

    public Packet WriteBytes(IEnumerable<byte> value)
    {
        var v = value as byte[] ?? value.ToArray();
        _data.AddRange(v);
        _position += v.Length;

        return this;
    }

    public Packet WriteShort(short value)
    {
        _data.Add((byte)(value & 0xFF));
        _data.Add((byte)((value >> 8) & 0xFF));
        _position += 2;

        return this;
    }

    public Packet WriteVarInt(int value)
    {
        while (true)
        {
            if ((value & 0xFFFFFF80) == 0)
            {
                WriteByte((byte)value);
                _position++;
                return this;
            }

            WriteByte((byte)((value & 0x7F) | 0x80));
            _position++;
            value >>= 7;
        }
    }

    public Packet WriteString(string value)
    {
        WriteVarInt(value.Length);
        foreach (var c in value) WriteByte((byte)c);

        return this;
    }

    public byte ReadByte()
    {
        var value = _data[_position];
        _position++;
        return value;
    }

    public byte[] ReadBytes(int length)
    {
        var value = _data.GetRange(_position, length).ToArray();
        _position += length;
        return value;
    }

    public short ReadShort()
    {
        var value = (short)(_data[_position] | (_data[_position + 1] << 8));
        _position += 2;
        return value;
    }

    public int ReadVarInt()
    {
        var value = 0;
        var shift = 0;
        while (true)
        {
            var b = ReadByte();
            value |= (b & 0x7F) << shift;
            if ((b & 0x80) != 0x80) return value;
            shift += 7;
        }
    }

    public string ReadString()
    {
        var length = ReadVarInt();
        var value = "";
        for (var i = 0; i < length; i++) value += (char)ReadByte();
        return value;
    }
}
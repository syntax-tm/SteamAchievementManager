using System;
using System.IO;
using System.Text;

namespace SAM.Serialization;

/// <summary>
/// Handles reading and writing strings to the named pipe
/// </summary>
public class PipeStream
{
    private readonly Stream stream;
    private readonly UnicodeEncoding encoding;

    public PipeStream(Stream stream)
    {
        this.stream = stream;
        encoding = new ();
    }

    public int ReadByte()
    {
        var value = stream.ReadByte();
        return value;
    }

    public void WriteByte(byte value)
    {
        stream.WriteByte(value);
        stream.Flush();
    }

    public void SendMessage(ApplicationMessage message)
    {
        var value = (byte) message;

        WriteByte(value);
    }

    public ApplicationMessage ReadMessage()
    {
        var value = ReadByte();

        if (!Enum.IsDefined(typeof(ApplicationMessage), value))
        {
            return ApplicationMessage.Error;
        }

        var message = (ApplicationMessage) value;
        return message;
    }

    public string ReadString()
    {
        var len = stream.ReadByte() * 256;
        len += stream.ReadByte();
        var buffer = new byte[len];
        var read = stream.Read(buffer, 0, len);

        return encoding.GetString(buffer);
    }

    public int WriteString(string value)
    {
        var buffer = encoding.GetBytes(value);
        var len = buffer.Length;
        if (len > ushort.MaxValue)
        {
            len = ushort.MaxValue;
        }
        stream.WriteByte((byte) (len / 256));
        stream.WriteByte((byte) (len & 255));
        stream.Write(buffer, 0, len);
        stream.Flush();

        return buffer.Length + 2;
    }
}

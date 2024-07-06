using System.Text;
using System.Linq;
using System.Collections.Generic;
using System;

namespace qASIC.Communication
{
    public class qPacket
    {
        public qPacket() : this(new byte[0]) { }

        public qPacket(byte[] bytes)
        {
            this.bytes = new List<byte>(bytes);
        }

        public List<byte> bytes;
        public int position;

        public byte[] ToArray() => bytes.ToArray();

        public byte[] ReadCurrentBytes(int length)
        {
            byte[] currentBytes = HasBytesFor(length) ?
                bytes.GetRange(position, length).ToArray() :
                new byte[length];

            position += length;
            return currentBytes;
        }

        public bool HasBytesFor(int length) =>
            bytes.Count - position >= length;

        public bool ReadBool() => BitConverter.ToBoolean(ReadCurrentBytes(1), 0);
        public byte ReadByte() => ReadCurrentBytes(1).First();
        public sbyte ReadSByte()
        {
            byte b = 0;
            var bytes = ReadCurrentBytes(1);
            if (bytes.Count() > 0)
                b = bytes.First();

            return unchecked((sbyte)b);
        }

        public int ReadInt() => BitConverter.ToInt32(ReadCurrentBytes(sizeof(int)), 0);
        public uint ReadUInt() => BitConverter.ToUInt32(ReadCurrentBytes(sizeof(uint)), 0);
        public float ReadFloat() => BitConverter.ToSingle(ReadCurrentBytes(sizeof(float)), 0);
        public double ReadDouble() => BitConverter.ToDouble(ReadCurrentBytes(sizeof(double)), 0);
        public long ReadLong() => BitConverter.ToInt64(ReadCurrentBytes(sizeof(long)), 0);
        public ulong ReadULong() => BitConverter.ToUInt64(ReadCurrentBytes(sizeof(ulong)), 0);
        public string ReadString()
        {
            var length = ReadInt();
            return length > 0 ?
                Encoding.UTF8.GetString(ReadCurrentBytes(length)) :
                string.Empty;
        }

        public T ReadNetworkSerializable<T>() where T : INetworkSerializable, new() =>
            (T)ReadNetworkSerializable(new T());

        public object ReadNetworkSerializable(INetworkSerializable serializable)
        {
            serializable.Read(this);
            return serializable;
        }

        public qPacket WriteBytes(params byte[] data)
        {
            bytes.AddRange(data);
            return this;
        }

        public qPacket Write(bool value) => WriteBytes(BitConverter.GetBytes(value));
        public qPacket Write(byte value) => WriteBytes(value);
        public qPacket Write(sbyte value) => WriteBytes(unchecked((byte)value));
        public qPacket Write(int value) => WriteBytes(BitConverter.GetBytes(value));
        public qPacket Write(uint value) => WriteBytes(BitConverter.GetBytes(value));
        public qPacket Write(float value) => WriteBytes(BitConverter.GetBytes(value));
        public qPacket Write(double value) => WriteBytes(BitConverter.GetBytes(value));
        public qPacket Write(long value) => WriteBytes(BitConverter.GetBytes(value));
        public qPacket Write(ulong value) => WriteBytes(BitConverter.GetBytes(value));

        public qPacket Write(string value)
        {
            if (value == string.Empty)
            {
                Write(0);
                return this;
            }

            byte[] textBytes = Encoding.UTF8.GetBytes(value);
            Write(textBytes.Length);
            return WriteBytes(textBytes);
        }

        public qPacket Write(INetworkSerializable item) =>
            item.Write(this);

        public override string ToString() =>
            $"Packet length:{bytes.Count} position:{position} bytes:{string.Join(",", bytes)}";
    }
}
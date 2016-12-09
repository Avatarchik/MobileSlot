using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;
using System.Net.Sockets;
using System.Linq;

using System.IO;

public class CrownGamesSocket : SlotSocket
{
    static private byte ZERO_BYTE = 0;
    static private byte[] END_BYTE = new byte[] { 0 };
    ArraySegment<byte> _endSegment = new ArraySegment<byte>(END_BYTE);

    public CrownGamesSocket(int BufferSize = 8192) : base(BufferSize)
    {

    }

    override protected IList<ArraySegment<byte>> CreateSendPacket(byte[] data)
    {
        List<ArraySegment<byte>> sendBuffers = new List<ArraySegment<byte>>();
        sendBuffers.Add(new ArraySegment<byte>(data));
        sendBuffers.Add(_endSegment);

        return sendBuffers;
    }

    override protected bool IsReceivedDone()
    {
        var count = END_BYTE.Length;

        if (_ms.Length < count) return false;

        var endBuffer = new byte[count];

        _ms.Position -= count;
        _ms.Read(endBuffer, 0, count);

        return endBuffer.SequenceEqual(END_BYTE);
    }
}

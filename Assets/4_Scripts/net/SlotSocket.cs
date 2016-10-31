using UnityEngine;
using System.Collections;

using System;
using System.Net;
using System.Net.Sockets;

// return "lbgames.sloticagames.com";
// "182.252.135.251";
//shining 13100
//highdia 13500
public class SlotSocket
{
    Socket _socket;
    public SlotSocket()
    {

    }

    void CreateSocket()
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }

    public void Connect(string host, int port)
    {
        IPHostEntry ipHostInfo = Dns.Resolve(host);
        IPAddress ipAddress = ipHostInfo.AddressList[0];
        IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

        if (_socket == null)
        {
            CreateSocket();
        }

        try
        {
			_socket.BeginConnect(remoteEP, new AsyncCallback( ConnectCallBack ),_socket);
        }
        catch (Exception ex)
        {
			ErrorLog( "connect fail", ex );
        }
    }

	void ConnectCallBack( IAsyncResult ar )
	{

	}
	
    public void Send()
    {

    }

	public void ErrorLog( string type, Exception ex )
	{
		
	}
}

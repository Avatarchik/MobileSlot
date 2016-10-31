using UnityEngine;
using System.Collections;
using lpesign;

public class GameServerCommunicator : SingletonSimple<GameServerCommunicator>
{
	SlotSocket _socket;
	void Start()
	{
		_socket = new SlotSocket();
	}

	public void Connect( string host, int port )
	{

	}

	public void Login()
	{

	}

	public void Spin()
	{

	}

	public void Send()
	{
		_socket.Send();
	}
}

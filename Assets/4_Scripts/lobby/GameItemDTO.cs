using UnityEngine;
using System.Collections;

public class GameItemDTO
{
	public enum GameItemTag
	{
		NEW,
		PLAY,
		COMINGSOON
	}

	public string Name{get;set;}
	public GameItemTag Tag{get;set;}
	public string IconPath{get;set;}
	public int ID{get;set;}
}

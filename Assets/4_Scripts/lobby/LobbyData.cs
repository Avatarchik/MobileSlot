using UnityEngine;
using System.Collections.Generic;

public class LobbyData
{
	public List<GameItemDTO> GameList{ get; private set;}



	public void SetTestDatas()
	{
		GameList = new List<GameItemDTO>();

		GameList.Add( new GameItemDTO
		{
			ID = 1,
			Name ="TestGame_1",
			Tag = GameItemDTO.GameItemTag.NEW,
			IconPath = ""
		});

		GameList.Add( new GameItemDTO
		{
			ID = 2,
			Name ="TestGame_2",
			Tag = GameItemDTO.GameItemTag.PLAY,
			IconPath = ""
		});

		GameList.Add( new GameItemDTO
		{
			ID = 3,
			Name ="TestGame_3",
			Tag = GameItemDTO.GameItemTag.COMINGSOON,
			IconPath = ""
		});


		GameList.Add( new GameItemDTO
		{
			ID = 4,
			Name ="TestGame_4",
			Tag = GameItemDTO.GameItemTag.COMINGSOON,
			IconPath = ""
		});

		GameList.Add( new GameItemDTO
		{
			ID = 5,
			Name ="TestGame_4",
			Tag = GameItemDTO.GameItemTag.COMINGSOON,
			IconPath = ""
		});
	}
}

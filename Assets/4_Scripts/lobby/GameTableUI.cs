using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameTableUI : MonoBehaviour
{
	[SerializeField]
	private GameItemUI _referenceGameItem;

	private ScrollRect _rect;
	private RectTransform _content;
	void Awake()
	{
		_referenceGameItem.gameObject.SetActive( false );

		_rect = GetComponent<ScrollRect>();
		_content = _rect.content;
	}

	void Start()
	{
		CreateGames();
	}

	void CreateGames()
	{
		List<GameItemDTO> list = Lobby.Instance.Data.GameList;
		var count = list.Count;
		for( var i  = 0; i < count; ++i )
		{
			GameItemDTO data = list[i];
			
			GameItemUI item = GameObject.Instantiate( _referenceGameItem);
			item.transform.SetParent( _content );
			item.gameObject.SetActive( true );
			item.transform.localScale = Vector3.one;
			item.SetData( data );
		}
	}
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameItemUI : MonoBehaviour
{
    public Image sprite;
    public Text gameName;

	private Toggle[] _toggles;

	private GameItemDTO _data;
	private Button _button;

	void Awake()
	{
		_toggles = GetComponentsInChildren<Toggle>();

		_button = GetComponent<Button>();
		_button.onClick.AddListener( OnClicked );
	}

    public void SetData(GameItemDTO data)
    {
		_data = data;

        gameName.text = data.Name;

		foreach( Toggle toggle in _toggles )
		{
			if( toggle.name == data.Tag.ToString())
			{
				toggle.isOn = true;
				break;
			}
		}

		switch( data.Tag )
		{
			case GameItemDTO.GameItemTag.COMINGSOON:
				_button.interactable = false;
				break;
			default:
				_button.interactable = true;
				break;
		}

		sprite.sprite = Resources.Load<Sprite>("lobby/game_icon_" + _data.ID.ToString("00"));
    }

	void OnClicked()
	{
		GameManager.Instance.GameLoad( _data );
	}
}

using UnityEngine;
using UnityEngine.UI;

[RequireComponent (typeof (RectTransform), typeof (Collider2D))]
public class Collider2DRaycastFilter : MonoBehaviour, ICanvasRaycastFilter
{
	Collider2D _collider;
	RectTransform _rtf;

	void Awake ()
	{
		_collider = GetComponent<Collider2D>();
		_rtf = GetComponent<RectTransform>();
	}

	#region ICanvasRaycastFilter implementation
	public bool IsRaycastLocationValid (Vector2 screenPos, Camera eventCamera)
	{
		var worldPoint = Vector3.zero;
		var isInside = RectTransformUtility.ScreenPointToWorldPointInRectangle(
			_rtf,
			screenPos,
			eventCamera,
			out worldPoint
		);
		if (isInside)
			isInside = _collider.OverlapPoint(worldPoint);

		return isInside;
	}
	#endregion
}
using UnityEngine;
using System.Collections;

namespace Game
{
    public class PaylineRenderer : MonoBehaviour
    {
        [SerializeField]
        int _index;

        public int Index { get { return _index; } }

        SpriteRenderer _renderer;

        void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();

            if (_renderer != null)
            {
                _renderer.sortingLayerName = Layers.Sorting.PAYLINE;
                _renderer.sortingOrder = _index;
            }
        }

        void Start()
        {
            Hide();
        }

        public void Show()
        {
            _renderer.enabled = true;
        }

        public void Hide()
        {
            _renderer.enabled = false;
        }
    }
}

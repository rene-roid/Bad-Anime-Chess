using UnityEngine;

namespace rene_roid
{
    public class Tile : MonoBehaviour
    {
        #region Internal
        [Header("Internal")]
        [SerializeField] private Color _baseColor, _offsetColor;
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private GameObject _highlight;

        #endregion

        #region External
        [Header("External")]
        public Piece Occupant = null;
        #endregion


        public void Init(bool isOffset)
        {
            _renderer.color = isOffset ? _offsetColor : _baseColor;
        }

        private void OnMouseEnter()
        {
            _highlight.SetActive(true);
        }

        private void OnMouseExit()
        {
            _highlight.SetActive(false);
        }
    }   
}

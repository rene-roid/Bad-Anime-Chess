using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace rene_roid
{
    public enum PieceType
    {
        Pawn,
        Rook,
        Knight,
        Bishop,
        Queen,
        King
    }

    public enum PieceColor
    {
        White,
        Black
    }
    public class Piece : MonoBehaviour
    {
        #region Internal
        [Header("Internal")]
        [SerializeField] private BoxCollider2D _collider;
        [SerializeField] private SpriteRenderer _renderer;

        [SerializeField] private Tile _currentTile;
        [SerializeField] private List<Tile> _legalTiles;
        private Board _board;
        private GameManager _gameManager;

        private bool _isMouseOver = false;
        private bool _isMovingPiece = false;
        private bool _selected = false;

        private bool _isFirstMove = true;
        #endregion

        #region External
        [Header("External")]
        public PieceType Type;
        public PieceColor ColorPiece;

        public Tile CurrentTile => _currentTile;

        public bool IsFirstMove
        {
            get { return _isFirstMove; }
            set { _isFirstMove = value; }
        }
        #endregion

        private void Start()
        {
            _collider = GetComponent<BoxCollider2D>();
            _renderer = GetComponent<SpriteRenderer>();

            _board = FindObjectOfType<Board>();
            _gameManager = FindObjectOfType<GameManager>();

            transform.position = new Vector3(_currentTile.transform.position.x, _currentTile.transform.position.y, -1);
        }

        private void Update()
        {
            if (_gameManager.State != GameManager.GameState.Playing) return;
            DropPieceUpdate();
            UpdateLegalTiles();
            
            if (ColorPiece == PieceColor.White && _gameManager.PlayerTurn != GameManager.Turn.White) return;
            if (ColorPiece == PieceColor.Black && _gameManager.PlayerTurn != GameManager.Turn.Black) return;
            
            SelectPieceUpdate();
            DragPieceUpdate();
        }

        private void OnMouseEnter()
        {
            print("Mouse Enter");
            _isMouseOver = true;
        }

        private void OnMouseExit()
        {
            print("Mouse Exit");
            _isMouseOver = false;
        }

        public void Init(PieceColor color, Tile spawnTile, PieceType type)
        {
            ColorPiece = color;
            _currentTile = spawnTile;
            _currentTile.Occupant = this;
            _legalTiles = new List<Tile>();

            Type = type;

            _renderer = GetComponent<SpriteRenderer>();
            _renderer.color = color == PieceColor.White ? Color.white : Color.gray;
            transform.position = new Vector3(_currentTile.transform.position.x, _currentTile.transform.position.y, -1);
        }

        #region Select Piece

        private void SelectPieceUpdate()
        {
            if (_isMouseOver && Input.GetMouseButtonDown(0) && !_selected)
            {
                _selected = true;
                _renderer.color = ColorPiece == PieceColor.White ? Color.yellow : Color.red;
                //_legalTiles = _board.GetLegalTiles(_currentTile, this);
                //_board.HighlightTiles(_legalTiles);
            }
            else if (!_isMouseOver && Input.GetMouseButtonDown(0) && _selected)
            {
                _selected = false;
                _renderer.color = ColorPiece == PieceColor.White ? Color.white : Color.gray;
                //_board.ClearHighlights();
            }
        }
        #endregion

        #region Move Piece

        private void DragPieceUpdate()
        {
            if (_isMouseOver && Input.GetMouseButton(0) && !_isMovingPiece) // Get piece from board
            {
                if (_board.CurrentPiece != null) return;
                _board.CurrentPiece = this;
                _isMovingPiece = true;
                _renderer.sortingOrder = _renderer.sortingOrder + 10;
            }

            if (Input.GetMouseButton(0) && _isMovingPiece) // Move piece to mouse pos
            {
                Debug.Log("Clicked on " + gameObject.name);
                // Move sprite to the cursor
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                transform.position = new Vector3(mousePos.x, mousePos.y, -1);
            }
        }

        private void DropPieceUpdate()
        {
            if (Input.GetMouseButtonUp(0) && _isMovingPiece)
            {
                Debug.Log("Released on " + gameObject.name);

                // Snap to nearest tile
                Tile[] tiles = Physics2D.OverlapBoxAll(transform.position, _collider.size, 0).Select(x => x.GetComponent<Tile>()).Where(x => x != null).ToArray();
                Tile nearestTile = tiles != null && tiles.Length > 0 ? tiles.OrderBy(x => Vector3.Distance(transform.position, x.transform.position)).First() : null;

                if (_legalTiles.Contains(nearestTile))
                {
                    if (nearestTile != null && nearestTile.Occupant != null && nearestTile.Occupant.ColorPiece != this.ColorPiece) // If tile has a different color piece
                    {
                        var piece = nearestTile.Occupant;
                        transform.position = new Vector3(nearestTile.transform.position.x, nearestTile.transform.position.y, -1);
                        _currentTile.Occupant = null;
                        KillPiece(nearestTile.Occupant);
                        nearestTile.Occupant = this;
                        _currentTile = nearestTile;

                        _selected = false;
                        _renderer.color = _selected ? (ColorPiece == PieceColor.White ? Color.yellow : Color.red) : (ColorPiece == PieceColor.White ? Color.white : Color.gray);
                        
                        _gameManager.ChangeTurn();
                        if (piece.Type == PieceType.King) _gameManager.KingKilled(piece.ColorPiece);
                        
                        if (_isFirstMove) _isFirstMove = false;
                    }
                    else if (nearestTile != null && nearestTile.Occupant == null) // If tile is empty
                    {
                        transform.position = new Vector3(nearestTile.transform.position.x, nearestTile.transform.position.y, -1);
                        _currentTile.Occupant = null;
                        nearestTile.Occupant = this;
                        _currentTile = nearestTile;

                        _selected = false;
                        _renderer.color = _selected ? (ColorPiece == PieceColor.White ? Color.yellow : Color.red) : (ColorPiece == PieceColor.White ? Color.white : Color.gray);

                        _gameManager.ChangeTurn();

                        IsPawnVulneableToEnPassant();

                        // En passant ----
                        IsPawnVulneableToEnPassant();

                        if (CanDoEnPassant)
                        {
                            if (EnPassantLeft != null)
                            {
                                var xpos = nearestTile.transform.position.x;
                                var ypos = EnPassantLeft.transform.position.y + (ColorPiece == PieceColor.White ? 1 : -1);
                                if (nearestTile.transform.position.x == xpos && nearestTile.transform.position.y == ypos)
                                {
                                    KillPiece(EnPassantLeft);
                                }
                            }

                            if (EnPassantRight != null)
                            {
                                var xpos = nearestTile.transform.position.x;
                                var ypos = EnPassantRight.transform.position.y + (ColorPiece == PieceColor.White ? 1 : -1);
                                if (nearestTile.transform.position.x == xpos && nearestTile.transform.position.y == ypos)
                                {
                                    KillPiece(EnPassantRight);
                                }
                            }

                            CanDoEnPassant = false;
                        }

                        /// --------------

                        if (_isFirstMove) _isFirstMove = false;
                    }
                    else
                    {
                        transform.position = new Vector3(_currentTile.transform.position.x, _currentTile.transform.position.y, -1);
                    }

                    CheckPawnPromotion();
                }
                else
                {
                    transform.position = new Vector3(_currentTile.transform.position.x, _currentTile.transform.position.y, -1);
                }

                _isMovingPiece = false;
                _board.CurrentPiece = null;

                _renderer.sortingOrder = _renderer.sortingOrder - 10;
            }
        }

        private void KillPiece(Piece piece)
        {
            print("Killed " + piece.gameObject.name);
            Destroy(piece.gameObject);
        }
        #endregion

        #region Legal Tiles

        private bool _frameSelected = false;
        private void UpdateLegalTiles()
        {
            if (_selected && !_frameSelected)
            {
                Invoke("SpawnTiles", 0.05f);
                _frameSelected = true;
            }
            else if (!_selected && _frameSelected)
            {
                //_board.ClearHighlights();
                _frameSelected = false;
                Helpers.DestroyChildren(_board.transform);
            }
        }

        private void SpawnTiles()
        {
            _legalTiles = _board.GetLegalTiles(_currentTile, this);
            //_board.HighlightTiles(_legalTiles);
        }
        #endregion

        #region Special Moves
        [Header("Special Moves")]
        public bool IsCastling = false;
        public bool PromotePawn = false;
        
        public bool IsEnPassant = false;
        public int EnPassantMove = 0;

        public bool CanDoEnPassant = false;
        public Piece EnPassantLeft = null;
        public Piece EnPassantRight = null;

        private void IsPawnVulneableToEnPassant()
        {
            if (Type != PieceType.Pawn) return;
            if (!_isFirstMove) return;

            print("Vulnerable to en passant in move: " + _gameManager.Moves);
            
            IsEnPassant = true;
            EnPassantMove = _gameManager.Moves;
        }

        private void CheckPawnPromotion()
        {
            if (Type == PieceType.Pawn && (ColorPiece == PieceColor.White && _currentTile.transform.position.y == 7 || ColorPiece == PieceColor.Black && _currentTile.transform.position.y == 0))
            {
                PromotePawn = true;

                // Instantiate a queen on the tile and destroy the pawn
                var queen = Instantiate(_board.QueenPrefab, _currentTile.transform.position, Quaternion.identity);
                queen._currentTile = _currentTile;
                queen.ColorPiece = ColorPiece;
                queen.Type = PieceType.Queen; 

                Destroy(gameObject);
            }
        }
        #endregion
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using UnityEngine;

namespace rene_roid
{
    public class Board : MonoBehaviour
    {
        #region Internal
        [Header("Internal")]
        [SerializeField] private int _width, _height;
        [SerializeField] private Tile _tilePrefab;
        [SerializeField] private List<Tile> _tiles;
        [SerializeField] private GameObject _legalTilePrefab;

        private GameManager _gameManager;
        #endregion

        #region External
        [Header("External")]
        public Piece CurrentPiece = null;

        public Piece PawnPrefab, RookPrefab, KnightPrefab, BishopPrefab, QueenPrefab, KingPrefab;
        #endregion


        private void Start()
        {
            _gameManager = FindObjectOfType<GameManager>();
            GenerateGrid();
        }

        private void GenerateGrid()
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    var spawnedTile = Instantiate(_tilePrefab, new Vector3(x, y, 1), Quaternion.identity);
                    //spawnedTile.name = $"Tile ({x}, {y})";

                    // Set tile name like in chess
                    spawnedTile.name = $"Tile {(char)('A' + x)}{y + 1}";

                    var isOffset = (x + y) % 2 == 0;
                    spawnedTile.Init(isOffset);

                    // Add spawned tiles to _tiles
                    _tiles.Add(spawnedTile);

                    print(spawnedTile.name);
                }
            }

            Helpers.Camera.transform.position = new Vector3(_width / 2, _height / 2, -10);
            GeneratePieces();
        }

        private void GeneratePieces()
        {
            // White pieces
            GeneratePiece(RookPrefab, 0, 0, PieceColor.White, PieceType.Rook);
            GeneratePiece(KnightPrefab, 1, 0, PieceColor.White, PieceType.Knight);
            GeneratePiece(BishopPrefab, 2, 0, PieceColor.White, PieceType.Bishop);
            GeneratePiece(QueenPrefab, 3, 0, PieceColor.White, PieceType.Queen);
            GeneratePiece(KingPrefab, 4, 0, PieceColor.White, PieceType.King);
            GeneratePiece(BishopPrefab, 5, 0, PieceColor.White, PieceType.Bishop);
            GeneratePiece(KnightPrefab, 6, 0, PieceColor.White, PieceType.Knight);
            GeneratePiece(RookPrefab, 7, 0, PieceColor.White, PieceType.Rook);

            for (int i = 0; i < 8; i++)
            {
                GeneratePiece(PawnPrefab, i, 1, PieceColor.White, PieceType.Pawn);
            }

            // Black pieces
            GeneratePiece(RookPrefab, 0, 7, PieceColor.Black, PieceType.Rook);
            GeneratePiece(KnightPrefab, 1, 7, PieceColor.Black, PieceType.Knight);
            GeneratePiece(BishopPrefab, 2, 7, PieceColor.Black, PieceType.Bishop);
            GeneratePiece(QueenPrefab, 3, 7, PieceColor.Black, PieceType.Queen);
            GeneratePiece(KingPrefab, 4, 7, PieceColor.Black, PieceType.King);
            GeneratePiece(BishopPrefab, 5, 7, PieceColor.Black, PieceType.Bishop);
            GeneratePiece(KnightPrefab, 6, 7, PieceColor.Black, PieceType.Knight);
            GeneratePiece(RookPrefab, 7, 7, PieceColor.Black, PieceType.Rook);

            for (int i = 0; i < 8; i++)
            {
                GeneratePiece(PawnPrefab, i, 6, PieceColor.Black, PieceType.Pawn);
            }
        }

        private void GeneratePiece(Piece piece, int x, int y, PieceColor color, PieceType type)
        {
            var spawnedPiece = Instantiate(piece, new Vector3(x, y, 0), Quaternion.identity);
            spawnedPiece.Init(color, GetTile(x, y), type);
        }

        public Tile GetTile(int x, int y)
        {
            var pos = new Vector3Int(x, y, 1);
            return _tiles.FirstOrDefault(t => t.transform.position == pos);
        }

        public List<Tile> GetLegalTiles(Tile tile, Piece piece)
        {
            var legalTiles = new List<Tile>();

            switch (piece.Type)
            {
                case PieceType.Pawn:
                    legalTiles = GetLegalTilesPawn(tile, piece);
                    break;
                case PieceType.Rook:
                    legalTiles = GetLegalTilesRook(tile, piece);
                    break;
                case PieceType.Knight:
                    legalTiles = GetLegalTilesKnight(tile, piece);
                    break;
                case PieceType.Bishop:
                    legalTiles = GetLegalTilesBishop(tile, piece);
                    break;
                case PieceType.Queen:
                    legalTiles = GetLegalTilesQueen(tile, piece);
                    break;
                case PieceType.King:
                    legalTiles = GetLegalTilesKing(tile, piece);
                    break;
            }

            foreach (var legalTile in legalTiles)
            {
                var spawnedTile = Instantiate(_legalTilePrefab, legalTile.transform.position, Quaternion.identity, this.transform);
                spawnedTile.name = $"Legal Tile ({legalTile.name})";
            }

            return legalTiles;
        }

        private List<Tile> GetLegalTilesRook(Tile tile, Piece rook)
        {
            var legalTiles = new List<Tile>();

            var x = tile.transform.position.x;
            var y = tile.transform.position.y;

            var stop = false;

            for (int i = 1; i < _width; i++)
            {
                var tileToCheck = GetTileAtPosition(new Vector3(x + i, y, 1));
                if (tileToCheck != null && !stop)
                {
                    if (tileToCheck.Occupant != null && tileToCheck.Occupant.ColorPiece == rook.ColorPiece)
                    {
                        stop = true;
                        continue;
                    }
                    legalTiles.Add(tileToCheck);
                }
            }

            stop = false;
            for (int i = 1; i < _width; i++)
            {
                var tileToCheck = GetTileAtPosition(new Vector3(x - i, y, 1));
                if (tileToCheck != null && !stop)
                {
                    if (tileToCheck.Occupant != null && tileToCheck.Occupant.ColorPiece == rook.ColorPiece)
                    {
                        stop = true;
                        continue;
                    }
                    legalTiles.Add(tileToCheck);
                }
            }

            stop = false;
            for (int i = 1; i < _height; i++)
            {
                var tileToCheck = GetTileAtPosition(new Vector3(x, y + i, 1));
                if (tileToCheck != null && !stop)
                {
                    if (tileToCheck.Occupant != null && tileToCheck.Occupant.ColorPiece == rook.ColorPiece)
                    {
                        stop = true;
                        continue;
                    }
                    legalTiles.Add(tileToCheck);
                }
            }

            stop = false;
            for (int i = 1; i < _height; i++)
            {
                var tileToCheck = GetTileAtPosition(new Vector3(x, y - i, 1));
                if (tileToCheck != null && !stop)
                {
                    if (tileToCheck.Occupant != null && tileToCheck.Occupant.ColorPiece == rook.ColorPiece)
                    {
                        stop = true;
                        continue;
                    }
                    legalTiles.Add(tileToCheck);
                }
            }

            return legalTiles;
        }

        private List<Tile> GetLegalTilesKnight(Tile tile, Piece knight)
        {
            var legalTiles = new List<Tile>();

            var x = tile.transform.position.x;
            var y = tile.transform.position.y;

            var tileToCheck = GetTileAtPosition(new Vector3(x + 1, y + 2, 1));
            if (tileToCheck != null && (tileToCheck.Occupant == null || tileToCheck.Occupant.ColorPiece != knight.ColorPiece))
            {
                legalTiles.Add(tileToCheck);
            }

            tileToCheck = GetTileAtPosition(new Vector3(x + 2, y + 1, 1));
            if (tileToCheck != null && (tileToCheck.Occupant == null || tileToCheck.Occupant.ColorPiece != knight.ColorPiece))
            {
                legalTiles.Add(tileToCheck);
            }

            tileToCheck = GetTileAtPosition(new Vector3(x + 2, y - 1, 1));
            if (tileToCheck != null && (tileToCheck.Occupant == null || tileToCheck.Occupant.ColorPiece != knight.ColorPiece))
            {
                legalTiles.Add(tileToCheck);
            }

            tileToCheck = GetTileAtPosition(new Vector3(x + 1, y - 2, 1));
            if (tileToCheck != null && (tileToCheck.Occupant == null || tileToCheck.Occupant.ColorPiece != knight.ColorPiece))
            {
                legalTiles.Add(tileToCheck);
            }

            tileToCheck = GetTileAtPosition(new Vector3(x - 1, y - 2, 1));
            if (tileToCheck != null && (tileToCheck.Occupant == null || tileToCheck.Occupant.ColorPiece != knight.ColorPiece))
            {
                legalTiles.Add(tileToCheck);
            }

            tileToCheck = GetTileAtPosition(new Vector3(x - 2, y - 1, 1));
            if (tileToCheck != null && (tileToCheck.Occupant == null || tileToCheck.Occupant.ColorPiece != knight.ColorPiece))
            {
                legalTiles.Add(tileToCheck);
            }

            tileToCheck = GetTileAtPosition(new Vector3(x - 2, y + 1, 1));
            if (tileToCheck != null && (tileToCheck.Occupant == null || tileToCheck.Occupant.ColorPiece != knight.ColorPiece))
            {
                legalTiles.Add(tileToCheck);
            }

            tileToCheck = GetTileAtPosition(new Vector3(x - 1, y + 2, 1));
            if (tileToCheck != null && (tileToCheck.Occupant == null || tileToCheck.Occupant.ColorPiece != knight.ColorPiece))
            {
                legalTiles.Add(tileToCheck);
            }

            return legalTiles;
        }

        private List<Tile> GetLegalTilesBishop(Tile tile, Piece bishop)
        {
            var legalTiles = new List<Tile>();

            var x = tile.transform.position.x;
            var y = tile.transform.position.y;

            var stop = false;

            for (int i = 1; i < _width; i++)
            {
                var tileToCheck = GetTileAtPosition(new Vector3(x + i, y + i, 1));
                if (tileToCheck != null && !stop)
                {
                    if (tileToCheck.Occupant != null && tileToCheck.Occupant.ColorPiece == bishop.ColorPiece)
                    {
                        stop = true;
                        continue;
                    }
                    else if (tileToCheck.Occupant != null && tileToCheck.Occupant.ColorPiece != bishop.ColorPiece)
                    {
                        legalTiles.Add(tileToCheck);
                        stop = true;
                        continue;
                    }
                    legalTiles.Add(tileToCheck);
                }
            }

            stop = false;
            for (int i = 1; i < _width; i++)
            {
                var tileToCheck = GetTileAtPosition(new Vector3(x - i, y - i, 1));
                if (tileToCheck != null && !stop)
                {
                    if (tileToCheck.Occupant != null && tileToCheck.Occupant.ColorPiece == bishop.ColorPiece)
                    {
                        stop = true;
                        continue;
                    }
                    else if (tileToCheck.Occupant != null && tileToCheck.Occupant.ColorPiece != bishop.ColorPiece)
                    {
                        legalTiles.Add(tileToCheck);
                        stop = true;
                        continue;
                    }
                    legalTiles.Add(tileToCheck);
                }
            }

            stop = false;
            for (int i = 1; i < _height; i++)
            {
                var tileToCheck = GetTileAtPosition(new Vector3(x + i, y - i, 1));
                if (tileToCheck != null && !stop)
                {
                    if (tileToCheck.Occupant != null && tileToCheck.Occupant.ColorPiece == bishop.ColorPiece)
                    {
                        stop = true;
                        continue;
                    }
                    else if (tileToCheck.Occupant != null && tileToCheck.Occupant.ColorPiece != bishop.ColorPiece)
                    {
                        legalTiles.Add(tileToCheck);
                        stop = true;
                        continue;
                    }
                    legalTiles.Add(tileToCheck);
                }
            }

            stop = false;
            for (int i = 1; i < _height; i++)
            {
                var tileToCheck = GetTileAtPosition(new Vector3(x - i, y + i, 1));
                if (tileToCheck != null && !stop)
                {
                    if (tileToCheck.Occupant != null && tileToCheck.Occupant.ColorPiece == bishop.ColorPiece)
                    {
                        stop = true;
                        continue;
                    }
                    else if (tileToCheck.Occupant != null && tileToCheck.Occupant.ColorPiece != bishop.ColorPiece)
                    {
                        legalTiles.Add(tileToCheck);
                        stop = true;
                        continue;
                    }
                    legalTiles.Add(tileToCheck);
                }
            }

            return legalTiles;
        }

        private List<Tile> GetLegalTilesQueen(Tile tile, Piece queen)
        {
            var legalTiles = new List<Tile>();

            var x = tile.transform.position.x;
            var y = tile.transform.position.y;

            var stop = false;
            for (int i = 1; i < _width; i++)
            {
                var tileToCheck = GetTileAtPosition(new Vector3(x + i, y + i, 1));
                if (tileToCheck != null && !stop)
                {
                    if (tileToCheck.Occupant != null && tileToCheck.Occupant.ColorPiece == queen.ColorPiece)
                    {
                        stop = true;
                        continue;
                    }
                    else if (tileToCheck.Occupant != null && tileToCheck.Occupant.ColorPiece != queen.ColorPiece)
                    {
                        legalTiles.Add(tileToCheck);
                        stop = true;
                        continue;
                    }
                    legalTiles.Add(tileToCheck);
                }
            }


            stop = false;
            for (int i = 1; i < _width; i++)
            {
                var tileToCheck = GetTileAtPosition(new Vector3(x - i, y - i, 1));
                if (tileToCheck != null && !stop)
                {
                    if (tileToCheck.Occupant != null && tileToCheck.Occupant.ColorPiece == queen.ColorPiece)
                    {
                        stop = true;
                        continue;
                    }
                    else if (tileToCheck.Occupant != null && tileToCheck.Occupant.ColorPiece != queen.ColorPiece)
                    {
                        legalTiles.Add(tileToCheck);
                        stop = true;
                        continue;
                    }
                    legalTiles.Add(tileToCheck);
                }
            }

            stop = false;
            for (int i = 1; i < _height; i++)
            {
                var tileToCheck = GetTileAtPosition(new Vector3(x + i, y - i, 1));
                if (tileToCheck != null && !stop)
                {
                    if (tileToCheck.Occupant != null && tileToCheck.Occupant.ColorPiece == queen.ColorPiece)
                    {
                        stop = true;
                        continue;
                    }
                    else if (tileToCheck.Occupant != null && tileToCheck.Occupant.ColorPiece != queen.ColorPiece)
                    {
                        legalTiles.Add(tileToCheck);
                        stop = true;
                        continue;
                    }
                    legalTiles.Add(tileToCheck);
                }
            }

            stop = false;
            for (int i = 1; i < _height; i++)
            {
                var tileToCheck = GetTileAtPosition(new Vector3(x - i, y + i, 1));
                if (tileToCheck != null && !stop)
                {
                    if (tileToCheck.Occupant != null && tileToCheck.Occupant.ColorPiece == queen.ColorPiece)
                    {
                        stop = true;
                        continue;
                    }
                    else if (tileToCheck.Occupant != null && tileToCheck.Occupant.ColorPiece != queen.ColorPiece)
                    {
                        legalTiles.Add(tileToCheck);
                        stop = true;
                        continue;
                    }
                    legalTiles.Add(tileToCheck);
                }
            }

            stop = false;
            for (int i = 1; i < _width; i++)
            {
                var tileToCheck = GetTileAtPosition(new Vector3(x + i, y, 1));
                if (tileToCheck != null && !stop)
                {
                    if (tileToCheck.Occupant != null && tileToCheck.Occupant.ColorPiece == queen.ColorPiece)
                    {
                        stop = true;
                        continue;
                    }
                    else if (tileToCheck.Occupant != null && tileToCheck.Occupant.ColorPiece != queen.ColorPiece)
                    {
                        legalTiles.Add(tileToCheck);
                        stop = true;
                        continue;
                    }
                    legalTiles.Add(tileToCheck);
                }
            }

            stop = false;
            for (int i = 1; i < _width; i++)
            {
                var tileToCheck = GetTileAtPosition(new Vector3(x - i, y, 1));
                if (tileToCheck != null && !stop)
                {
                    if (tileToCheck.Occupant != null && tileToCheck.Occupant.ColorPiece == queen.ColorPiece)
                    {
                        stop = true;
                        continue;
                    }
                    else if (tileToCheck.Occupant != null && tileToCheck.Occupant.ColorPiece != queen.ColorPiece)
                    {
                        legalTiles.Add(tileToCheck);
                        stop = true;
                        continue;
                    }
                    legalTiles.Add(tileToCheck);
                }
            }

            stop = false;
            for (int i = 1; i < _height; i++)
            {
                var tileToCheck = GetTileAtPosition(new Vector3(x, y + i, 1));
                if (tileToCheck != null && !stop)
                {
                    if (tileToCheck.Occupant != null && tileToCheck.Occupant.ColorPiece == queen.ColorPiece)
                    {
                        stop = true;
                        continue;
                    }
                    else if (tileToCheck.Occupant != null && tileToCheck.Occupant.ColorPiece != queen.ColorPiece)
                    {
                        legalTiles.Add(tileToCheck);
                        stop = true;
                        continue;
                    }
                    legalTiles.Add(tileToCheck);
                }
            }

            stop = false;
            for (int i = 1; i < _height; i++)
            {
                var tileToCheck = GetTileAtPosition(new Vector3(x, y - i, 1));
                if (tileToCheck != null && !stop)
                {
                    if (tileToCheck.Occupant != null && tileToCheck.Occupant.ColorPiece == queen.ColorPiece)
                    {
                        stop = true;
                        continue;
                    }
                    else if (tileToCheck.Occupant != null && tileToCheck.Occupant.ColorPiece != queen.ColorPiece)
                    {
                        legalTiles.Add(tileToCheck);
                        stop = true;
                        continue;
                    }
                    legalTiles.Add(tileToCheck);
                }
            }

            return legalTiles;
        }

        private List<Tile> GetLegalTilesKing(Tile tile, Piece king)
        {
            var legalTiles = new List<Tile>();

            var x = tile.transform.position.x;
            var y = tile.transform.position.y;

            var tileToCheck = GetTileAtPosition(new Vector3(x + 1, y, 1));
            if (tileToCheck != null && (tileToCheck.Occupant == null || tileToCheck.Occupant.ColorPiece != king.ColorPiece))
            {
                legalTiles.Add(tileToCheck);
            }

            tileToCheck = GetTileAtPosition(new Vector3(x + 1, y + 1, 1));
            if (tileToCheck != null && (tileToCheck.Occupant == null || tileToCheck.Occupant.ColorPiece != king.ColorPiece))
            {
                legalTiles.Add(tileToCheck);
            }

            tileToCheck = GetTileAtPosition(new Vector3(x, y + 1, 1));
            if (tileToCheck != null && (tileToCheck.Occupant == null || tileToCheck.Occupant.ColorPiece != king.ColorPiece))
            {
                legalTiles.Add(tileToCheck);
            }

            tileToCheck = GetTileAtPosition(new Vector3(x - 1, y + 1, 1));
            if (tileToCheck != null && (tileToCheck.Occupant == null || tileToCheck.Occupant.ColorPiece != king.ColorPiece))
            {
                legalTiles.Add(tileToCheck);
            }

            tileToCheck = GetTileAtPosition(new Vector3(x - 1, y, 1));
            if (tileToCheck != null && (tileToCheck.Occupant == null || tileToCheck.Occupant.ColorPiece != king.ColorPiece))
            {
                legalTiles.Add(tileToCheck);
            }

            tileToCheck = GetTileAtPosition(new Vector3(x - 1, y - 1, 1));
            if (tileToCheck != null && (tileToCheck.Occupant == null || tileToCheck.Occupant.ColorPiece != king.ColorPiece))
            {
                legalTiles.Add(tileToCheck);
            }

            tileToCheck = GetTileAtPosition(new Vector3(x, y - 1, 1));
            if (tileToCheck != null && (tileToCheck.Occupant == null || tileToCheck.Occupant.ColorPiece != king.ColorPiece))
            {
                legalTiles.Add(tileToCheck);
            }

            tileToCheck = GetTileAtPosition(new Vector3(x + 1, y - 1, 1));
            if (tileToCheck != null && (tileToCheck.Occupant == null || tileToCheck.Occupant.ColorPiece != king.ColorPiece))
            {
                legalTiles.Add(tileToCheck);
            }

            return legalTiles;
        }

        private List<Tile> GetLegalTilesPawn(Tile tile, Piece pawn)
        {
            var legalTiles = new List<Tile>();

            var x = tile.transform.position.x;
            var y = tile.transform.position.y;

            var tileToCheck = GetTileAtPosition(new Vector3(x, y + (pawn.ColorPiece == PieceColor.White ? 1 : -1), 1));
            if (tileToCheck != null && tileToCheck.Occupant == null)
            {
                legalTiles.Add(tileToCheck);
            }

            tileToCheck = GetTileAtPosition(new Vector3(x + 1, y + (pawn.ColorPiece == PieceColor.White ? 1 : -1), 1));
            if (tileToCheck != null && (tileToCheck.Occupant != null && tileToCheck.Occupant.ColorPiece != pawn.ColorPiece)) legalTiles.Add(tileToCheck);

            tileToCheck = GetTileAtPosition(new Vector3(x - 1, y + (pawn.ColorPiece == PieceColor.White ? 1 : -1), 1));
            if (tileToCheck != null && (tileToCheck.Occupant != null && tileToCheck.Occupant.ColorPiece != pawn.ColorPiece)) legalTiles.Add(tileToCheck);

            if (pawn.IsFirstMove)
            {
                tileToCheck = GetTileAtPosition(new Vector3(x, y + (pawn.ColorPiece == PieceColor.White ? 2 : -2), 1));
                if (tileToCheck != null && tileToCheck.Occupant == null)
                {
                    legalTiles.Add(tileToCheck);
                }
            }

            // En passant
            var tileToCheckLeft = GetTileAtPosition(new Vector3(x - 1, pawn.ColorPiece == PieceColor.White ? 4 : 3, 1));
            var tileToCheckRight = GetTileAtPosition(new Vector3(x + 1, pawn.ColorPiece == PieceColor.White ? 4 : 3, 1));

            if (tileToCheckLeft != null && tileToCheckLeft.Occupant != null && tileToCheckLeft.Occupant.ColorPiece != pawn.ColorPiece && tileToCheckLeft.Occupant.IsEnPassant && tileToCheckLeft.Occupant.Type == PieceType.Pawn)
            {
                if (pawn.CurrentTile.transform.position.y == (pawn.ColorPiece == PieceColor.White ? 4 : 3))
                {
                    print("Tile on the left is true with " + tileToCheckLeft.Occupant.Type + " and " + tileToCheckLeft.Occupant.ColorPiece);
                    print("Current move is " + _gameManager.Moves + " and move needed is " + (tileToCheckLeft.Occupant.EnPassantMove));
                    // Check if en passant is possible
                    if (tileToCheckLeft.Occupant.EnPassantMove == _gameManager.Moves)
                    {
                        print("En passant is possible");
                        var tileToCheckEnPassant = GetTileAtPosition(new Vector3(x - 1, y + (pawn.ColorPiece == PieceColor.White ? 1 : -1), 1));
                        if (tileToCheckEnPassant != null && tileToCheckEnPassant.Occupant == null)
                        {
                            legalTiles.Add(tileToCheckEnPassant);

                            pawn.CanDoEnPassant = true;
                            pawn.EnPassantLeft = tileToCheckLeft.Occupant;
                        }
                    }
                }
            }

            if (tileToCheckRight != null && tileToCheckRight.Occupant != null && tileToCheckRight.Occupant.ColorPiece != pawn.ColorPiece && tileToCheckRight.Occupant.IsEnPassant && tileToCheckRight.Occupant.Type == PieceType.Pawn)
            {
                print(pawn.CurrentTile.transform.position.y + " " + (pawn.ColorPiece == PieceColor.White ? 4 : 3));
                if (pawn.CurrentTile.transform.position.y == (pawn.ColorPiece == PieceColor.White ? 4 : 3))
                {
                    print("Tile on the right is true with " + tileToCheckRight.Occupant.Type + " and " + tileToCheckRight.Occupant.ColorPiece);
                    print("Current move is " + _gameManager.Moves + " and move needed is " + (tileToCheckRight.Occupant.EnPassantMove));
                    // Check if en passant is possible
                    if (tileToCheckRight.Occupant.EnPassantMove == _gameManager.Moves)
                    {
                        print("En passant is possible");
                        var tileToCheckEnPassant = GetTileAtPosition(new Vector3(x + 1, y + (pawn.ColorPiece == PieceColor.White ? 1 : -1), 1));
                        if (tileToCheckEnPassant != null && tileToCheckEnPassant.Occupant == null)
                        {
                            legalTiles.Add(tileToCheckEnPassant);

                            pawn.CanDoEnPassant = true;
                            pawn.EnPassantRight = tileToCheckRight.Occupant;
                        }
                    }
                }
            }

            return legalTiles;
        }

        private Tile GetTileAtPosition(Vector3 position) // returns the tile at a given position
        {
            var tile = _tiles.FirstOrDefault(t => t.transform.position == position);
            return tile;
        }
    }
}
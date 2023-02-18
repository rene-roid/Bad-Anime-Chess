using UnityEngine;

namespace rene_roid {    
    public class GameManager : PersistentSingleton<GameManager>
    {
        public enum GameState { MainMenu, Playing, Paused, GameOver }
        public enum Turn { White, Black, None }
        
        #region Internal
        [Header("Internal")]
        [SerializeField] private int _moves = 0;

        #endregion

        #region External
        [Header("External")]
        public Turn PlayerTurn = Turn.None;
        public GameState State = GameState.MainMenu;

        public int Moves { get { return _moves; } set { _moves = value; } }
        #endregion

        void Start()
        {
            
        }
        
        void Update()
        {
            UpdateGameState();
        }

        private void UpdateGameState()
        {
            switch (State)
            {
                case GameState.MainMenu:
                    break;
                case GameState.Playing:
                    break;
                case GameState.Paused:
                    break;
                case GameState.GameOver:
                    break;
                default:
                    break;
            }
        }

        public void ChangeGameState(GameState newState)
        {
            // Exit current state
            switch (State)
            {
                case GameState.MainMenu:
                    break;
                case GameState.Playing:
                    break;
                case GameState.Paused:
                    break;
                case GameState.GameOver:
                    break;
                default:
                    break;
            }

            // Enter current state
            switch (newState)
            {
                case GameState.MainMenu:
                    break;
                case GameState.Playing:
                    break;
                case GameState.Paused:
                    break;
                case GameState.GameOver:
                    break;
                default:
                    break;
            }

            State = newState;
        }

        public void ChangeTurn()
        {
            // If current turn is white change it to black and viceversa
            PlayerTurn = PlayerTurn == Turn.White ? Turn.Black : Turn.White;

            _moves++;
        }

        public void KingKilled(PieceColor color)
        {
            if (color == PieceColor.White)
            {
                // Black won
            }

            if (color == PieceColor.Black)
            {
                // White won
            }

            ChangeGameState(GameState.GameOver);
        }
    }
}

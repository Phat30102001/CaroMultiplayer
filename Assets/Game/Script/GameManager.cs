using System.Collections.Generic;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    public enum GameMode { OfflineLocal, OnlineMatch }

    [Header("Configurations")]
    [SerializeField] private GameMode selectedMode;
    [SerializeField] private int boardSize = 15;

    [Header("References")]
    [SerializeField] private BoardView boardView;
    [SerializeField] private Slot slotPrefab;

    private GameBoard _gameBoard;
    private EndGameCheckingManager _winChecker;
    private List<IPlayerController> _players;
    private int _currentPlayerIndex;
    private bool _isGameActive = true;

    private void Start()
    {
        // 1. Initialize core logic components
        _gameBoard = new GameBoard(boardSize);
        _winChecker = new EndGameCheckingManager();

        // 2. Data binding: Automatic UI update on data change
        _gameBoard.OnCellChanged += boardView.UpdateSlotVisual;

        // 3. Generate the board UI
        float slotSize = slotPrefab.GetComponent<RectTransform>().sizeDelta.x;
        boardView.GenerateBoard(boardSize, slotSize);

        // 4. Setup game mode (Inject appropriate player controllers)
        SetupGameMode(selectedMode);

        // 5. Start the first turn
        StartNextTurn();
    }

    private void SetupGameMode(GameMode mode)
    {
        _players = new List<IPlayerController>();

        if (mode == GameMode.OfflineLocal)
        {
            // OFFLINE MODE: Both X and O get input from the mouse on this device
            _players.Add(new LocalPlayerController(CellState.X, boardView));
            _players.Add(new LocalPlayerController(CellState.O, boardView));
        }
        else if (mode == GameMode.OnlineMatch)
        {
            // ONLINE MODE: You are X (Local mouse), Opponent is O (Network)
            _players.Add(new LocalPlayerController(CellState.X, boardView));
            _players.Add(new NetworkPlayerController(CellState.O));
            
            // Note: If you are the client joining later, you can swap them:
            // _players.Add(new NetworkPlayerController(CellState.X));
            // _players.Add(new LocalPlayerController(CellState.O, boardView));
        }
        _currentPlayerIndex = 0;
    }

    private void StartNextTurn()
    {
        if (!_isGameActive) return;

        IPlayerController activePlayer = _players[_currentPlayerIndex];
        Debug.Log($"Waiting for move from player: {activePlayer.Sign}");

        // Request move coordinates from the active player
        activePlayer.RequestMove(_gameBoard, (coords) =>
        {
            ExecuteMove(coords, activePlayer.Sign);
        });
    }

    private void ExecuteMove(Vector2Int coords, CellState sign)
    {
        // Attempt to place the piece into the data matrix
        if (_gameBoard.SetCell(coords.x, coords.y, sign))
        {
            // If playing online, send the coordinates to the network server here!
            // Example: if(sign == MyLocalSign) NetworkManager.SendMove(coords);

            // Check for win condition
            if (_winChecker.CheckWin(_gameBoard, coords, sign))
            {
                Debug.Log($"GAME OVER! Player {sign} has WON!");
                _isGameActive = false;
                return;
            }

            // Switch turn to the next player
            _currentPlayerIndex = (_currentPlayerIndex + 1) % _players.Count;
            StartNextTurn();
        }
        else
        {
            // If cell is occupied or invalid, request the same player to pick another cell
            StartNextTurn();
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events when destroyed to prevent memory leaks
        if (_gameBoard != null) 
            _gameBoard.OnCellChanged -= boardView.UpdateSlotVisual;
    }
}
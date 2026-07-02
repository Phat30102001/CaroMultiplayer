// File: GameOrchestrator.cs (Fully Compatible with Online/Offline)
using System.Collections.Generic;
using UnityEngine;

public class GameOrchestrator : MonoBehaviour
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
    
    private CellState _localPlayerSign; // Tracking the local player identity in online matches

    private void Start()
    {
        // Core systems initialization
        _gameBoard = new GameBoard(boardSize);
        _winChecker = new EndGameCheckingManager();
        _gameBoard.OnCellChanged += boardView.UpdateSlotVisual;

        float slotSize = slotPrefab.GetComponent<RectTransform>().sizeDelta.x;
        boardView.GenerateBoard(boardSize, slotSize);

        // If playing offline, startup instantly. If online, wait for network initialization.
        if (selectedMode == GameMode.OfflineLocal)
        {
            SetupOfflineMode();
            StartNextTurn();
        }
    }

    private void SetupOfflineMode()
    {
        _players = new List<IPlayerController>
        {
            new LocalPlayerController(CellState.X, boardView),
            new LocalPlayerController(CellState.O, boardView)
        };
        _currentPlayerIndex = 0;
    }

    /// <summary>
    /// Invoked by NetworkCaroManager once network roles are randomly distributed.
    /// </summary>
    public void InitNetworkMatch(CellState assignedLocalSign)
    {
        _localPlayerSign = assignedLocalSign;
        _players = new List<IPlayerController>();

        // Order matters: X always occupies index 0 and moves first.
        // We inject controllers dynamically based on who scored the X sign.
        if (_localPlayerSign == CellState.X)
        {
            _players.Add(new LocalPlayerController(CellState.X, boardView)); // Me (Goes first)
            _players.Add(new NetworkPlayerController(CellState.O));        // Opponent
        }
        else
        {
            _players.Add(new NetworkPlayerController(CellState.X));        // Opponent (Goes first)
            _players.Add(new LocalPlayerController(CellState.O, boardView)); // Me
        }

        _currentPlayerIndex = 0;
        StartNextTurn();
    }

    private void StartNextTurn()
    {
        if (!_isGameActive) return;

        IPlayerController activePlayer = _players[_currentPlayerIndex];
        
        activePlayer.RequestMove(_gameBoard, (coords) =>
        {
            // Execute locally if it's valid data
            ExecuteMove(coords, activePlayer.Sign);
        });
    }

    // Inside File: GameOrchestrator.cs -> ExecuteMove method

    private void ExecuteMove(Vector2Int coords, CellState sign)
    {
        if (_gameBoard.SetCell(coords.x, coords.y, sign))
        {
            // IF ONLINE: If the move belongs to the LOCAL user, send it to the global network manager
            if (selectedMode == GameMode.OnlineMatch && sign == _localPlayerSign)
            {
                // Fixed naming to match the strict In-Scene Singleton
                NetworkGameManager.Instance.BroadcastMoveServerRpc(coords, sign);
            }

            if (_winChecker.CheckWin(_gameBoard, coords, sign))
            {
                Debug.Log($"GAME OVER! Player {sign} has WON!");
                _isGameActive = false;
                return;
            }

            _currentPlayerIndex = (_currentPlayerIndex + 1) % _players.Count;
            StartNextTurn();
        }
        else
        {
            StartNextTurn();
        }
    }

    /// <summary>
    /// Triggered via ClientRpc when move coordinates arrive from the network.
    /// </summary>
    public void OnReceiveNetworkMove(Vector2Int coords, CellState sign)
    {
        // Filter out our own mirrored moves since they've already been executed locally
        if (selectedMode == GameMode.OnlineMatch && sign != _localPlayerSign)
        {
            if (_players[_currentPlayerIndex] is NetworkPlayerController networkPlayer)
            {
                networkPlayer.ReceiveMoveFromNetwork(coords);
            }
        }
    }

    private void OnDestroy()
    {
        if (_gameBoard != null) 
            _gameBoard.OnCellChanged -= boardView.UpdateSlotVisual;
    }
}
// File: GameOrchestrator.cs (Fully Adaptive for Rematching)
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
    private CellState _localPlayerSign;

    private void Start()
    {
        _winChecker = new EndGameCheckingManager();

        float slotSize = slotPrefab.GetComponent<RectTransform>().sizeDelta.x;
        boardView.GenerateBoard(boardSize, slotSize);

        if (selectedMode == GameMode.OfflineLocal)
        {
            SetupOfflineMode();
            StartNextTurn();
        }
    }

    private void SetupOfflineMode()
    {
        // Safe unsubscribe if re-initializing
        if (_gameBoard != null) _gameBoard.OnCellChanged -= boardView.UpdateSlotVisual;

        _gameBoard = new GameBoard(boardSize);
        _gameBoard.OnCellChanged += boardView.UpdateSlotVisual;
        boardView.ResetAllSlots();

        _players = new List<IPlayerController>
        {
            new LocalPlayerController(CellState.X, boardView),
            new LocalPlayerController(CellState.O, boardView)
        };
        _currentPlayerIndex = 0;
        _isGameActive = true;
    }

    public void InitNetworkMatch(CellState assignedLocalSign)
    {
        // 1. Clean data tracking and unbind from old board event instances to prevent memory bloating
        if (_gameBoard != null)
        {
            _gameBoard.OnCellChanged -= boardView.UpdateSlotVisual;
        }

        // 2. Instantiate a fresh model layer matrix data
        _gameBoard = new GameBoard(boardSize);
        _gameBoard.OnCellChanged += boardView.UpdateSlotVisual;

        // 3. Command the UI View layer to wipe out the old icons
        boardView.ResetAllSlots();

        // 4. Update orchestrator state flags
        _localPlayerSign = assignedLocalSign;
        _isGameActive = true;
        _players = new List<IPlayerController>();

        // 5. Inject controllers based on who scored the X sign randomly
        if (_localPlayerSign == CellState.X)
        {
            _players.Add(new LocalPlayerController(CellState.X, boardView));
            _players.Add(new NetworkPlayerController(CellState.O));
        }
        else
        {
            _players.Add(new NetworkPlayerController(CellState.X));
            _players.Add(new LocalPlayerController(CellState.O, boardView));
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
            ExecuteMove(coords, activePlayer.Sign);
        });
    }

    private void ExecuteMove(Vector2Int coords, CellState sign)
    {
        if (_gameBoard.SetCell(coords.x, coords.y, sign))
        {
            if (selectedMode == GameMode.OnlineMatch && sign == _localPlayerSign)
            {
                NetworkGameManager.Instance.BroadcastMoveServerRpc(coords, sign);
            }

            if (_winChecker.CheckWin(_gameBoard, coords, sign))
            {
                _isGameActive = false;
                HandleGameEnd(sign);
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

    private void HandleGameEnd(CellState winnerSign)
    {
        Debug.Log($"[GameOrchestrator] Player {winnerSign} has won the match.");

        if (selectedMode == GameMode.OnlineMatch)
        {
            // Determine text message for the local player
            string message = (winnerSign == _localPlayerSign) ? "YOU WIN!" : "YOU LOSE!";
            
            // Pop up the consensus UI panel
            RematchPanelUI.Instance.ShowPanel(message);
        }
        else
        {
            // Offline fallback
            SetupOfflineMode();
            StartNextTurn();
        }
    }

    public void OnReceiveNetworkMove(Vector2Int coords, CellState sign)
    {
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
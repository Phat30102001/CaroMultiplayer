// File: NetworkGameManager.cs
using Unity.Netcode;
using UnityEngine;

public class NetworkGameManager : NetworkBehaviour
{
    public static NetworkGameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameOrchestrator orchestrator;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Host listens to connection events to know when a client joins
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

            // Safety check: If a client somehow connected instantly
            CheckAndStartMatch();
        }
    }

    public override void OnNetworkDespawn()
    {
        // Clean up the event subscription when the network shuts down to prevent memory leaks
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        // Triggered whenever ANY client connects (including the Host themselves as ID 0)
        Debug.Log($"[NetworkGameManager] Client connected with ID: {clientId}");
        
        // Check if we have enough players to start the match
        CheckAndStartMatch();
    }

    private void CheckAndStartMatch()
    {
        // Caro is a 2-player game. We wait until exactly 2 players are in the network session
        if (NetworkManager.Singleton.ConnectedClients.Count == 2)
        {
            Debug.Log("[NetworkGameManager] Both players are present! Starting role randomization...");
            DeterminePlayerRoles();
        }
    }

    private void DeterminePlayerRoles()
    {
        // Randomize who gets X (goes first) and O (goes second)
        CellState hostRole = (Random.value > 0.5f) ? CellState.X : CellState.O;
        CellState clientRole = (hostRole == CellState.X) ? CellState.O : CellState.X;

        // Broadcast the decision to ALL connected clients currently in the room
        AssignRolesClientRpc(hostRole, clientRole);
    }

    [ClientRpc]
    private void AssignRolesClientRpc(CellState hostRole, CellState clientRole)
    {
        // Identify what sign this specific local machine is responsible for
        CellState myAssignedSign = IsServer ? hostRole : clientRole;
        
        Debug.Log($"[NetworkGameManager] Match started. Your local sign: {myAssignedSign}");
        
        // Notify the orchestrator to kick off the game loop on BOTH machines simultaneously
        orchestrator.InitNetworkMatch(myAssignedSign);
    }

    [ServerRpc(RequireOwnership = false)]
    public void BroadcastMoveServerRpc(Vector2Int coords, CellState sign)
    {
        ReceiveMoveClientRpc(coords, sign);
    }

    [ClientRpc]
    private void ReceiveMoveClientRpc(Vector2Int coords, CellState sign)
    {
        orchestrator.OnReceiveNetworkMove(coords, sign);
    }
}
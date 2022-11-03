using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class LobbyManager : NetworkBehaviour
{

    public LobbyPlayerPanel playerPanelPrefab;
    public GameObject playerScrollContent;
    public TMPro.TMP_Text txtPlayerNumber;
    public Button btnStart;
    public Button btnReady;
    public Player playerPrefab;

    private NetworkList<PlayerInfo> allPlayers = new NetworkList<PlayerInfo>();
    private List<LobbyPlayerPanel> playerPanels = new List<LobbyPlayerPanel>();

    private Color[] playerColors = new Color[] {
        Color.blue,
        Color.green,
        Color.yellow,
        Color.grey,
        Color.cyan
    };
    private int colorIndex = 0;


    public void Start() {
        if (IsHost) {
            AddPlayerToList(NetworkManager.LocalClientId);
            RefreshPlayerPanels();
        }

        if (IsClient) {
            btnReady.onClick.AddListener(ClientOnReadyClicked);
        }
    }

    public override void OnNetworkSpawn() {
        if (IsHost) {
            NetworkManager.Singleton.OnClientConnectedCallback += HostOnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HostOnClientDisconnected;
            btnStart.gameObject.SetActive(false);  
        }

        // Must be after Host Connects to signals
        base.OnNetworkSpawn();

        if (IsClient && !IsHost){
            allPlayers.OnListChanged += ClientOnAllPlayersChanged;
            btnStart.gameObject.SetActive(false);
        }
        txtPlayerNumber.text = $"Player #{NetworkManager.LocalClientId}";
    }


    [ServerRpc(RequireOwnership = false)]
    public void TogglerReadyServerRpc(ServerRpcParams serverRpcParams = default) {
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        int playerIndex = FindPlayerIndex(clientId);
        PlayerInfo info = allPlayers[playerIndex];

        info.isReady = !info.isReady;
        allPlayers[playerIndex] = info;

        info = allPlayers[playerIndex];

        int readyCount = 0;
        foreach (PlayerInfo readyInfo in allPlayers) {
            if (readyInfo.isReady) {
                readyCount += 1;
            }
        }

        btnStart.enabled = readyCount == allPlayers.Count - 1;
    }
    // -----------------
    // Private
    // -----------------
    private void AddPlayerToList(ulong clientId) {
        allPlayers.Add(new PlayerInfo(clientId, NextColor(), true));
    }

    private void AddPlayerPanel(PlayerInfo info) {
        LobbyPlayerPanel newPanel = Instantiate(playerPanelPrefab);
        newPanel.transform.SetParent(playerScrollContent.transform, false);
        newPanel.SetName($"Player {info.clientId.ToString()}");
        newPanel.SetColor(info.color);
        newPanel.SetReady(info.isReady);
        playerPanels.Add(newPanel);

    }

    private void RefreshPlayerPanels() {
        foreach (LobbyPlayerPanel panel in playerPanels) {
            Destroy(panel.gameObject);
        }
        playerPanels.Clear();

        foreach (PlayerInfo pi in allPlayers) {
            AddPlayerPanel(pi);
        }
    }

    private int FindPlayerIndex(ulong clientId) {
        var idx = 0;
        var found = false;

        while (idx < allPlayers.Count && !found) {
            if (allPlayers[idx].clientId == clientId) {
                found = true;
            } else {
                idx += 1;
            }
        }
        
        if (!found) {
            idx = -1;
        }

        return idx;
    }
    
    private Color NextColor() {
    Color newColor = playerColors[colorIndex];
        colorIndex += 1;
        if (colorIndex > playerColors.Length - 1) {
            colorIndex = 0;
        }
        return newColor;
    }
    // ------------------
    // Events
    // ------------------
    private void ClientOnAllPlayersChanged(NetworkListEvent<PlayerInfo> changeEvent) {
        RefreshPlayerPanels();
    }

    private void HostOnClientConnected(ulong clientId) {
        btnStart.enabled = false;
        AddPlayerToList(clientId);
        RefreshPlayerPanels();
    }
    
    private void HostOnClientDisconnected(ulong clientId) {
        int index = FindPlayerIndex(clientId);
        if(index != -1) {
            allPlayers.RemoveAt(index);
            RefreshPlayerPanels();
        }
    }

    private void ClientOnReadyClicked() {
        TogglerReadyServerRpc();
    }
}


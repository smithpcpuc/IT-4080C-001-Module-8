using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;


public class ChatUI : NetworkBehaviour {

    public TMPro.TMP_Text txtChatLog;
    public Button btnSend;
    public TMPro.TMP_InputField inputMessage;

    public void Start() {
        btnSend.onClick.AddListener(ClientOnSendClicked);
        inputMessage.onSubmit.AddListener(ClientOnInputSubmit);
    }

    public override void OnNetworkSpawn() {
        txtChatLog.text = "-- Start Chat Log --";
        if (IsHost) {
            NetworkManager.Singleton.OnClientConnectedCallback += HostOnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HostOnClientDisconnected;
            DisplayMessageLocally("You are the host!");
        } else {
            DisplayMessageLocally($"You are Player #{NetworkManager.Singleton.LocalClientId}!");
        }

    }

    private void SendUIMessage() {
        string msg = inputMessage.text;
        inputMessage.text = "";
        SendChatMessageServerRpc(msg);
    }

    //----------------------
    //Events
    //----------------------
    private void HostOnClientConnected(ulong clientId) {
        SendChatMessageClientRpc($"Client {clientId} connected");
    }

    private void HostOnClientDisconnected(ulong clientId) {
        SendChatMessageClientRpc($"Client {clientId} disconnected");
    }

    public void ClientOnSendClicked() {
        SendUIMessage();
    }

    public void ClientOnInputSubmit(string text){
        SendUIMessage();
    }

    //---------------------
    // RPC
    //---------------------

    [ClientRpc]
    public void SendChatMessageClientRpc(string message, ClientRpcParams clientRpcParams = default){
        DisplayMessageLocally(message);
    }


    [ServerRpc(RequireOwnership = false)]
    public void SendChatMessageServerRpc(string message, ServerRpcParams serverRpcParams = default){
        Debug.Log($"Host got message:  {message}");
        string newMessage = $"[Player #{serverRpcParams.Receive.SenderClientId}]:  {message}";
        SendChatMessageClientRpc(newMessage);
    }
    public void DisplayMessageLocally(string message) {
        Debug.Log(message);
        txtChatLog.text += $"\n{message}";
    }
}

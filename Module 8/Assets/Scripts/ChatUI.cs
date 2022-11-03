using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Unity.Netcode;

public class ChatUI : NetworkBehaviour {
    public override void OnNetworkSpawn() {
        if (IsHost) {
            SendChatMessageClientRpc("I am the host");
        } else {
            SendChatMessageServerRpc("I am a client");
        }
    }

    [ClientRpc]
    public void SendChatMessageClientRpc(string message, ClientRpcParams clientRpParams = default){
        Debug.Log(message);
    }



    [ServerRpc(RequireOwnership = false)]
    public void SendChatMessageServerRpc(string message, ServerRpcParams serverRpcParams = default){
        Debug.Log($"Host got message:  {message}");
    }
}

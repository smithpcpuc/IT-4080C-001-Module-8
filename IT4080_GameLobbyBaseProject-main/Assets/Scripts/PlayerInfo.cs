using UnityEngine;
using Unity.Netcode;
using UnityEngine.Serialization;
using Unity.Collections;

public struct PlayerInfo : INetworkSerializable, System.IEquatable<PlayerInfo> {
    public ulong clientId;

    public PlayerInfo(ulong id, Color c, bool isReady=false) {
        clientId = id;
        
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
        serializer.SerializeValue(ref clientId);
    }

    public bool Equals(PlayerInfo other) {
        return other.clientId == clientId;
    }
}
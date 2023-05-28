using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using Unity.Collections;

public struct LobbyPlayerState : INetworkSerializable, IEquatable<LobbyPlayerState>
{
    // Start is called before the first frame update
    public ulong ClientId;
    public FixedString64Bytes PlayerName;
    public bool IsReady;
    public int CharacterId;
    public bool InGame;



    public LobbyPlayerState(ulong clientId, FixedString64Bytes playerName, bool isReady, int characterId, bool inGame)
    {
        ClientId = clientId;
        PlayerName = playerName;
        IsReady = isReady;
        CharacterId = characterId;
        InGame = inGame;
    }

    public bool Equals(LobbyPlayerState other)
    {
        return ClientId == other.ClientId && PlayerName == other.PlayerName && IsReady == other.IsReady && CharacterId==other.CharacterId && InGame==other.InGame;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref PlayerName);
        serializer.SerializeValue(ref IsReady);
        serializer.SerializeValue(ref CharacterId);
        serializer.SerializeValue(ref InGame);
    }

}

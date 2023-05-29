using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using Unity.Collections;

public struct LobbyPlayerState : INetworkSerializable, IEquatable<LobbyPlayerState>
{
    //Struct que guarda la informacion de los jugadores del lobby
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

    public bool Equals(LobbyPlayerState other) //Metodo necesario de la interfaz IEquatable
    {
        return ClientId == other.ClientId && PlayerName == other.PlayerName && IsReady == other.IsReady && CharacterId==other.CharacterId && InGame==other.InGame;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter //Metodo necesario de la interfaz INetworkSerializable
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref PlayerName);
        serializer.SerializeValue(ref IsReady);
        serializer.SerializeValue(ref CharacterId);
        serializer.SerializeValue(ref InGame);
    }

}

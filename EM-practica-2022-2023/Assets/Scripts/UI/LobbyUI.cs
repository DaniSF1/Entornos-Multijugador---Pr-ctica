using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Lobby.UI { 
    public class LobbyUI : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private LobbyPlayer[] lobbyPlayerCards;
        [SerializeField] private Button startGameButton;
        [SerializeField] private TMP_InputField playerName;
        [SerializeField] private GameObject lobbyPanel;
        public static NetworkVariable<int> playerCount;

        private NetworkList<LobbyPlayerState> lobbyPlayers;

        private void Awake()
        {
            lobbyPlayers = new NetworkList<LobbyPlayerState>();
            playerCount = new NetworkVariable<int>();
            playerCount.Value = 0;
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer) {
                Debug.Log("Is Host");
                lobbyPlayers.OnListChanged += HandleLobbyPlayersStateChanged;

                startGameButton.gameObject.SetActive(true);

                NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;

                foreach(NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
                {
                    HandleClientConnected(client.ClientId);
                    
                }
            }
            else if (IsClient)
            {
                Debug.Log("Is Client");
                lobbyPlayers.OnListChanged += HandleLobbyPlayersStateChanged;

            }
        }

        public override void OnNetworkDespawn()
        { 
            if (IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
            }
            else if(IsClient)
            {
                lobbyPlayers.OnListChanged -= HandleLobbyPlayersStateChanged;

            }
        }

        private bool EveryoneReady()
        {
            if(lobbyPlayers.Count < 2)
            {
                return false;
            }

            foreach (var player in lobbyPlayers)
            {
                if (!player.IsReady)
                {
                    return false;
                }
            }

            return true;
        }

        private void HandleClientDisconnect(ulong clientId)
        {
            for (int i = 0; i<lobbyPlayers.Count; i++)
            {
                if (lobbyPlayers[i].ClientId == clientId)
                {
                    lobbyPlayers.RemoveAt(i);
                    playerCount.Value = lobbyPlayers.Count;
                    break;
                }
            }
        }

        private void HandleClientConnected(ulong clientId)
        {
            foreach (var player in lobbyPlayers) { if (player.ClientId == clientId) { return;  } }
            Debug.Log(clientId);
            lobbyPlayers.Add(new LobbyPlayerState(clientId, "Player_"+clientId, false, 0, false));
            playerCount.Value = lobbyPlayers.Count;
        }

        public LobbyPlayerState GetPlayer(ulong clientId)
        {
            for (int i = 0; i < lobbyPlayers.Count; i++)
            {
                if (lobbyPlayers[i].ClientId == clientId)
                {
                    return lobbyPlayers[i];
                }
            }
            return new LobbyPlayerState();
        }

        [ServerRpc (RequireOwnership = false)]
        private void ToggleReadyServerRpc(ServerRpcParams serverRpcParams = default)
        {
            for (int i = 0; i < lobbyPlayers.Count; i++)
            {
                if (lobbyPlayers[i].ClientId == serverRpcParams.Receive.SenderClientId) { 
                    lobbyPlayers[i] = new LobbyPlayerState(lobbyPlayers[i].ClientId, lobbyPlayers[i].PlayerName, 
                        !lobbyPlayers[i].IsReady, lobbyPlayers[i].CharacterId, lobbyPlayers[i].InGame);
                }
            }
        }

        [ServerRpc (RequireOwnership = false)]
        private void SwapChatacterServerRpc(ServerRpcParams serverRpcParams = default)
        {
            for (int i = 0; i < lobbyPlayers.Count; i++)
            {
                if (lobbyPlayers[i].ClientId == serverRpcParams.Receive.SenderClientId)
                {
                    int id = (lobbyPlayers[i].CharacterId + 1) % 3;
                    lobbyPlayers[i] = new LobbyPlayerState(lobbyPlayers[i].ClientId, lobbyPlayers[i].PlayerName, lobbyPlayers[i].IsReady, id, lobbyPlayers[i].InGame);
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void StartGameServerRpc(ServerRpcParams serverRpcParams = default)
        {
            if(serverRpcParams.Receive.SenderClientId != NetworkManager.Singleton.LocalClientId) { return; }

            if(!EveryoneReady()) { return; }

            for (int i = 0; i < lobbyPlayers.Count; i++)
            {
                lobbyPlayers[i] = new LobbyPlayerState(lobbyPlayers[i].ClientId, lobbyPlayers[i].PlayerName,
                       lobbyPlayers[i].IsReady, lobbyPlayers[i].CharacterId, true);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void NameChangeServerRpc(string name, ServerRpcParams serverRpcParams)
        {
            for (int i = 0; i < lobbyPlayers.Count; i++)
            {
                if (lobbyPlayers[i].ClientId == serverRpcParams.Receive.SenderClientId)
                {
                    lobbyPlayers[i] = new LobbyPlayerState(lobbyPlayers[i].ClientId, name, 
                        lobbyPlayers[i].IsReady, lobbyPlayers[i].CharacterId, lobbyPlayers[i].InGame);
                }
            }
        }

        public void OnNameChanged(string name)
        {
            NameChangeServerRpc(name, default);
        }

        public void OnReadyClicked()
        {
            ToggleReadyServerRpc();
        }

        public void OnStartGameClicked()
        {
            StartGameServerRpc();
        }

        public void OnSelectCharacter()
        {
            SwapChatacterServerRpc();
        }

        private void HandleLobbyPlayersStateChanged(NetworkListEvent<LobbyPlayerState> changeEvent)
        {
            for(int i= 0; i<lobbyPlayerCards.Length; i++)
            {
                if (lobbyPlayers.Count > i)
                {
                    lobbyPlayerCards[i].UdpateDisplay(lobbyPlayers[i]);
                }
                else
                {
                    lobbyPlayerCards[i].DisableDisplay();
                }

                if (IsHost)
                {
                    startGameButton.interactable = EveryoneReady();
                }
            }
        }
    }
}

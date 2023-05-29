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
        [SerializeField] private LobbyPlayer[] lobbyPlayerCards; //Gestiona la tarjeta de cada jugador
        [SerializeField] private Button startGameButton;
        [SerializeField] private TMP_InputField playerName;
        [SerializeField] private GameObject lobbyPanel; 
        public static NetworkVariable<int> playerCount; //Cuenta la cantidad de jugadores que hay en el lobby, sirve para la funcion de sincronizar
                                                        //el inicio

        private NetworkList<LobbyPlayerState> lobbyPlayers; //Guarda una lista con todos los jugadores en el lobby

        private void Awake()
        {
            lobbyPlayers = new NetworkList<LobbyPlayerState>();
            playerCount = new NetworkVariable<int>();
            playerCount.Value = 0;
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer) {
                lobbyPlayers.OnListChanged += HandleLobbyPlayersStateChanged; //Suscribe una funcion cuando se modifique la lista

                startGameButton.gameObject.SetActive(true); //El boton de empezar partida esta activa solo para el servidor

                //Cuando se conecte o desconecte un cliente invoca unas funciones que lo manejan
                NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;

                foreach(NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
                {
                    HandleClientConnected(client.ClientId); //Si hay una lista de clientes conectados los conecta al servidor
                    
                }
            }
            else if (IsClient)
            {
                lobbyPlayers.OnListChanged += HandleLobbyPlayersStateChanged; //Suscribe una funcion cuando se modifique la lista

            }
        }

        public override void OnNetworkDespawn() //Desuscribe los eventos cuando se cierra
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

        private bool EveryoneReady() //Comprueba si hay más de 1 jugador y si están listos
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

        private void HandleClientDisconnect(ulong clientId) //Para cuando se desconecte un jugador, la lista lo elimina
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

        private void HandleClientConnected(ulong clientId) //Cuando se conecta un jugador la lista lo añade creando un nuevo LobbyPlayerState
        {
            foreach (var player in lobbyPlayers) { if (player.ClientId == clientId) { return;  } }
            
            lobbyPlayers.Add(new LobbyPlayerState(clientId, "Player_"+clientId, false, 0, false));
            playerCount.Value = lobbyPlayers.Count;
        }

        public LobbyPlayerState GetPlayer(ulong clientId) //Dependiendo de un id de cliente devuelve el lobbyplayerstate asociado en la lista
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

        [ServerRpc (RequireOwnership = false)] //Cuando pulsa el jugador que está listo su variable IsReady se invierte de valor
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

        [ServerRpc (RequireOwnership = false)] //Cuando el jugador le da al boton de cambiar el personaje se modifica el ChatacterId
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

        [ServerRpc(RequireOwnership = false)] //Cuando se le da a empezar partida todos los jugadores cambian su valor InGame a true
        private void StartGameServerRpc(ServerRpcParams serverRpcParams = default)
        {
            if(serverRpcParams.Receive.SenderClientId != NetworkManager.Singleton.LocalClientId) { return; }

            if(!EveryoneReady()) { return; } //Si no están todos listos no funciona

            for (int i = 0; i < lobbyPlayers.Count; i++)
            {
                lobbyPlayers[i] = new LobbyPlayerState(lobbyPlayers[i].ClientId, lobbyPlayers[i].PlayerName,
                       lobbyPlayers[i].IsReady, lobbyPlayers[i].CharacterId, true);
            }
        }

        [ServerRpc(RequireOwnership = false)] //Cuando se le da a empezar partida todos los jugadores cambian su valor InGame a true
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

        //Todos los siguientes eventos son intermedios entre los botones de la interfaz y el propio codigo. Invocan funciones Rpc
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
        //---------------------------------------------
        private void HandleLobbyPlayersStateChanged(NetworkListEvent<LobbyPlayerState> changeEvent) //Cuando se cambia la lista de cualquier forma se actualiza
                                                                                                    //la interfaz (LobbyPlayer)                                                                        
        {
            for(int i= 0; i<lobbyPlayerCards.Length; i++)
            {
                if (lobbyPlayers.Count > i)
                {
                    lobbyPlayerCards[i].UdpateDisplay(lobbyPlayers[i]); //Actualiza la interfaz
                }
                else
                {
                    lobbyPlayerCards[i].DisableDisplay(); //Desactiva la tarjeta del jugador que se ha ido
                }

                if (IsHost)
                {
                    startGameButton.interactable = EveryoneReady(); //Hace el boton start interactuable si todos estan listos
                }
            }
        }
    }
}

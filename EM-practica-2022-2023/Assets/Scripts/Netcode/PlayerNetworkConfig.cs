using Lobby.UI;
using System;
using TMPro;
using UI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Netcode
{
    public class PlayerNetworkConfig : NetworkBehaviour
    {
        private GameObject characterPrefab;      //Variable para el prefab del jugador
        private GameObject characterGameObject;
        public GameObject Oni;
        public GameObject Huntress;
        public GameObject Akai_Kaze;

        public override void OnNetworkSpawn()
        {
            LobbyPlayer.OnGameStart += InstantiateCharacter;
            //if (!IsOwner) return;
            //InstantiateCharacterServerRpc(OwnerClientId);   //Si no el jugador no es el host, hacemos una llamada RCP para el servidor
        }

        public void InstantiateCharacter(LobbyPlayerState playerData)
        {
            if (!IsOwner) return;
            InstantiateCharacterServerRpc(OwnerClientId, playerData);
            characterGameObject.GetComponentInChildren<DisplayName>().SetNamesClientRpc(Convert.ToString(playerData.PlayerName));

        }

        public override void OnDestroy()
        {
            if (!IsServer) return;
            GameManager.RemovePlayer(characterGameObject);
            base.OnDestroy();
        }

        [ServerRpc]
        public void InstantiateCharacterServerRpc(ulong id, LobbyPlayerState playerData)
        {
            if (id != playerData.ClientId) return;
            switch(playerData.CharacterId)
            {
                case 0:
                    getHuntress();
                    break;
                case 1:
                    getOni();
                    break;
                case 2:
                    getAkai();
                    break;  
            }
            if (characterGameObject == null) { 
                characterGameObject = Instantiate(characterPrefab);              //Tomamos el prefab del personaje y lo hacemos un gameobject
                GameManager.AddPlayer(characterGameObject);

                characterGameObject.GetComponent<NetworkObject>().SpawnWithOwnership(id);   //Tomamos el networkobject del cliente y 
                characterGameObject.transform.SetParent(transform, false);                  //Colocamos al cliente en el mapa
            }
        }

        public void getOni()
        {
            characterPrefab = Oni;
        }
        public void getHuntress()
        {
            characterPrefab = Huntress;
        }
        public void getAkai()
        {
            characterPrefab = Akai_Kaze;
        }
    }
}

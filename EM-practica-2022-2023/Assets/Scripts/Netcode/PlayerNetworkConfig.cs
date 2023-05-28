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
                characterGameObject = Instantiate(characterPrefab);   //Tomamos el prefab del personaje y lo hacemos un gameobject
                characterGameObject.name = playerData.PlayerName.ToString();
                GameManager.AddPlayer(characterGameObject);
                characterGameObject.GetComponent<NetworkObject>().SpawnWithOwnership(id);   //Tomamos el networkobject del cliente y
                transform.position = new Vector3(getPosX(id), 2.7f, 0);
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
        public float getPosX(ulong id)
        {
            float pos = 0f;
            int idx = (int) id % 4;
            switch(idx) 
            {
                case 0:
                    pos = -8f;
                    break; 
                case 1:
                    pos = -2f;
                    break;
                case 2:
                    pos = 3f;
                    break;
                case 3:
                    pos = 8f;
                    break;
            }
            return pos;
        }
    }
}

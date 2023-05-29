using Cinemachine;
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
        private Vector3 startPos;

        public override void OnNetworkSpawn()
        {
            GameManager.onGameRestart += PosRestart;
            LobbyPlayer.OnGameStart += InstantiateCharacter;
            //if (!IsOwner) return;
            //InstantiateCharacterServerRpc(OwnerClientId);   //Si no el jugador no es el host, hacemos una llamada RCP para el servidor
        }

        public void InstantiateCharacter(LobbyPlayerState playerData)
        {
            if (!IsOwner) return;
            InstantiateCharacterServerRpc(OwnerClientId, playerData);
            //characterGameObject.GetComponentInChildren<DisplayName>().SetNamesClientRpc(Convert.ToString(playerData.PlayerName));

        }

        public override void OnDestroy()
        {
            if (!IsServer) return;
            Debug.Log("Me desconecto");
            GameManager.RemoveDisconectedPlayer(characterGameObject);
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
                GameManager.AddPlayer(characterGameObject);
                characterGameObject.GetComponent<NetworkObject>().SpawnWithOwnership(id);   //Tomamos el networkobject del cliente y
                startPos = new Vector3(getPosX(id), 2.7f, 0);
                transform.position = startPos;
                characterGameObject.transform.SetParent(transform, false);                  //Colocamos al cliente en el mapa
            }
        }

        
        public void PosRestart()
        {
            Debug.Log("Reinicia Pos");
            if(characterGameObject == null) { return; }
            characterGameObject.transform.position = startPos;
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

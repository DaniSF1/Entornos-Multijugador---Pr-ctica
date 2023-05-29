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
        private GameObject characterPrefab;         //Variable para el prefab del jugador
        private GameObject characterGameObject;     //GameObject del prefab ya instanciado del jugador 
        public GameObject Oni;
        public GameObject Huntress;
        public GameObject Akai_Kaze;                //Cargamos los prefabs de los personajes para manejarlos mejor
        private Vector3 startPos;                   //Auxilizar que nos indica la posicion donde empieza cada jugador
        public static Action playerLoaded;          //Delegado para cuando un jugador se carga. Usado en el GameManager. Lo usamos para el inicio sincronizado

        public override void OnNetworkSpawn()       //Cuando aparece un jugador...
        {
            GameManager.onGameRestart += PosRestart;            //Suscribimos el metodo PosRestart al delegado onGameRestart de GameManager para reiniciar la partida
            LobbyPlayer.OnGameStart += InstantiateCharacter;    //Suscribimos el metodo InstantiateCharacter al delegado OnGameStart de LobbyPlayer para reiniciar la partida
                                                                //Cuando el juego comienza, se instancia los personajes correspondientes
            //if (!IsOwner) return;
            //InstantiateCharacterServerRpc(OwnerClientId);   //Si no el jugador no es el host, hacemos una llamada RCP para el servidor
        }

        public void InstantiateCharacter(LobbyPlayerState playerData)   //Cuando instanciamos un jugador...
        {
            if (!IsOwner) return;
            InstantiateCharacterServerRpc(OwnerClientId, playerData);   //Lo instanciamos solo en el servidor
        }

        public override void OnDestroy()                                //Override de OnDestroy. Cuando se desconecta un jugador...
        {
            if (!IsServer) return;
            Debug.Log("Me desconecto");                     
            GameManager.RemoveDisconectedPlayer(characterGameObject);   //Lo sacamos de las listas correspondientes del GameManager
            base.OnDestroy();                                           //Llamamos al metodo onDestroy base para que siga con normalidad
        }

        [ServerRpc]
        public void InstantiateCharacterServerRpc(ulong id, LobbyPlayerState playerData)    //Instanciamos en el servidor con la info del jugador el personaje
        {
            if (id != playerData.ClientId) return;
            switch(playerData.CharacterId)          //Seleccionamos el personaje del jugador a traves de la informacion que nos llega del lobby
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

            if (characterGameObject == null) {      //Si el personaje es nulo...
                characterGameObject = Instantiate(characterPrefab);                         //Tomamos el prefab del personaje y lo instanciamos en un gameobject
                characterGameObject.name = playerData.PlayerName.ToString();                //Cambiamos el nombre del gameObject al del jugador
                GameManager.AddPlayer(characterGameObject);                                 //Añadimos toda la informacion al GameManager
                characterGameObject.GetComponent<NetworkObject>().SpawnWithOwnership(id);   //Tomamos el networkobject del cliente y
                startPos = new Vector3(getPosX(id), 2.7f, 0);                               
                transform.position = startPos;                                              //Usamos el auxiliar para establecer su posicion inicial
                characterGameObject.transform.SetParent(transform, false);                  //Colocamos al cliente en el mapa
                playerLoaded?.Invoke();                                                     //Cuando se carga el jugador, llamamos al metodo correspondiente del GameManager
                                                                                            //para así hacer el inicio sincronizado 
            }
        }

        
        public void PosRestart()                                    //Reiniciamos la posicion del jugador
        {
            Debug.Log("Reinicia Pos");
            if(characterGameObject == null) { return; }
            characterGameObject.transform.position = startPos;
        }

        public void getOni()                                        //Tomamos el prefab que el jugador haya elegido
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
        public float getPosX(ulong id)                              //Dependiendo de la id del jugador, aparecerá en una posicion u otra
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

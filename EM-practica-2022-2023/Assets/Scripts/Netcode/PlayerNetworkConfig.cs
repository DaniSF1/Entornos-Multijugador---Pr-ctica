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
            if (!IsOwner) return;
            InstantiateCharacterServerRpc(OwnerClientId);   //Si no el jugador no es el host, hacemos una llamada RCP para el servidor
        }

        public override void OnDestroy()
        {
            if (!IsServer) return;
            GameManager.RemovePlayer(characterGameObject);
            base.OnDestroy();
        }

        [ServerRpc]
        public void InstantiateCharacterServerRpc(ulong id)
        {
            if (characterPrefab == null) characterPrefab = Huntress;
            characterGameObject = Instantiate(characterPrefab);                         //Tomamos el prefab del personaje y lo hacemos un gameobject
            GameManager.AddPlayer(characterGameObject);
            characterGameObject.GetComponent<NetworkObject>().SpawnWithOwnership(id);   //Tomamos el networkobject del cliente y 
            characterGameObject.transform.SetParent(transform, false);                  //Colocamos al cliente en el mapa
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

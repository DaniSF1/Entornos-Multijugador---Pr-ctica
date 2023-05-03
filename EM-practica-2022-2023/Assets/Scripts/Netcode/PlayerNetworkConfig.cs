using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Netcode
{
    public class PlayerNetworkConfig : NetworkBehaviour
    {
        public GameObject characterPrefab;      //Variable para el prefab del jugador

        public override void OnNetworkSpawn()
        {
            if (!IsOwner) return;
            InstantiateCharacterServerRpc(OwnerClientId);   //Si no el jugador no es el host, hacemos una llamada RCP para el servidor
        }
    
        [ServerRpc]
        public void InstantiateCharacterServerRpc(ulong id)
        {
            GameObject characterGameObject = Instantiate(characterPrefab);              //Tomamos el prefab del personaje y lo hacemos un gameobject
            characterGameObject.GetComponent<NetworkObject>().SpawnWithOwnership(id);   //Tomamos el networkobject del cliente y 
            characterGameObject.transform.SetParent(transform, false);                  //Colocamos al cliente en el mapa
        }
    }
}

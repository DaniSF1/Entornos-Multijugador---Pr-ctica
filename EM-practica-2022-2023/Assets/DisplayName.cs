using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;

public class DisplayName : MonoBehaviour
{

    [SerializeField] private TMP_Text displayName;

    [ClientRpc]
    public void SetNamesClientRpc(string name)
    {
        displayName.text = name;
    }


}

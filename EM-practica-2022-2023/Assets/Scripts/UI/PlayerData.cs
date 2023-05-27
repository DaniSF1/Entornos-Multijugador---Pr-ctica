using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public struct PlayerData 
{
    public FixedString64Bytes PlayerName { get; set; }

    public PlayerData(FixedString64Bytes playerName)
    {
        PlayerName = playerName;    
    }
}

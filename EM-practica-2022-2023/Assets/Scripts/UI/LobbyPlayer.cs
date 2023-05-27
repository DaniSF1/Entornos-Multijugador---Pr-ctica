using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayer : MonoBehaviour
{
    [SerializeField] private GameObject waitingForPlayerPanel;
    [SerializeField] private GameObject playerDataPanel;

    [SerializeField] private TMP_Text playerDisplayNameText;
    [SerializeField] private Image selectedCharacter;
    [SerializeField] private Toggle isReadyToggle;
    [SerializeField] private GameObject lobbyPanel;


    [SerializeField] private Sprite[] characterImages;

    public static Action<LobbyPlayerState> OnGameStart;
    public void UdpateDisplay(LobbyPlayerState lobbyPlayerState)
    {
        playerDisplayNameText.text = Convert.ToString(lobbyPlayerState.PlayerName);
        isReadyToggle.isOn = lobbyPlayerState.IsReady;

        lobbyPanel.SetActive(!lobbyPlayerState.InGame);
        if (lobbyPlayerState.InGame == true) { OnGameStart?.Invoke(lobbyPlayerState); }
        
        waitingForPlayerPanel.SetActive(false);
        playerDataPanel.SetActive(true);
        Debug.Log(lobbyPlayerState.CharacterId);
        selectedCharacter.sprite = characterImages[lobbyPlayerState.CharacterId];
    }

    public void DisableDisplay()
    {
        waitingForPlayerPanel.SetActive(true);
        playerDataPanel.SetActive(false);
    }
}

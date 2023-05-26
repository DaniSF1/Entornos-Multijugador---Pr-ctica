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

    [SerializeField] private Sprite[] characterImages;

    public void UdpateDisplay(LobbyPlayerState lobbyPlayerState)
    {
        playerDisplayNameText.text = Convert.ToString(lobbyPlayerState.PlayerName);
        isReadyToggle.isOn = lobbyPlayerState.IsReady;


        waitingForPlayerPanel.SetActive(false);
        playerDataPanel.SetActive(true);
        selectedCharacter.sprite = characterImages[lobbyPlayerState.CharacterId];
    }

    public void DisableDisplay()
    {
        waitingForPlayerPanel.SetActive(true);
        playerDataPanel.SetActive(false);
    }
}

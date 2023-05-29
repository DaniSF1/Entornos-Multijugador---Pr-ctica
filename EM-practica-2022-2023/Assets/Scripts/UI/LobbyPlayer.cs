using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayer : MonoBehaviour
{
    //Esta clase se encarga de gestionar las tarjetitas y la interfaz del lobby, cada tarjeta tiene este script
    [SerializeField] private GameObject waitingForPlayerPanel;
    [SerializeField] private GameObject playerDataPanel;

    [SerializeField] private TMP_Text playerDisplayNameText;
    [SerializeField] private Image selectedCharacter;
    [SerializeField] private Toggle isReadyToggle;
    [SerializeField] private GameObject lobbyPanel;


    [SerializeField] private Sprite[] characterImages;

    public static Action<LobbyPlayerState> OnGameStart; //Delegado para avisar al resto de clases que se ha iniciado la partida
    public void UdpateDisplay(LobbyPlayerState lobbyPlayerState) //Esta funcion se ejecuta a nivel de cliente y actualiza la intefaz del lobby, 
                                                                 //dependerá del estado del jugador que se le pase cambiarlo de una forma u otra. 
    {
        playerDisplayNameText.text = Convert.ToString(lobbyPlayerState.PlayerName); //Cambia el nombre del jugador en el lobby
        isReadyToggle.isOn = lobbyPlayerState.IsReady; //Marca o Desmarca el toggle de listo

        lobbyPanel.SetActive(!lobbyPlayerState.InGame); //Si ha empezado partida se desactiva, si ha terminado se activa
        if (lobbyPlayerState.InGame == true) { OnGameStart?.Invoke(lobbyPlayerState); } //Si los jugadores están en la partida invoca el delegado
        
        //Hace aparecer el jugador conectado en una de las casillas
        waitingForPlayerPanel.SetActive(false);
        playerDataPanel.SetActive(true);

        //Cambia el personaje que aparece en la tarjeta de cada jugador depende del que seleccione.
        selectedCharacter.sprite = characterImages[lobbyPlayerState.CharacterId];
    }

    public void DisableDisplay() //Se encarga de actualizar la interfaz del lobby para cuando un jugador se desconecta
    {
        waitingForPlayerPanel.SetActive(true);
        playerDataPanel.SetActive(false);
    }
}

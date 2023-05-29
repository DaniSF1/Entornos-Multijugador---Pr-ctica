using JetBrains.Annotations;
using Lobby.UI;
using Movement.Components;
using Netcode;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Systems;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    public static List<string> currentNames = new List<string>();
    public static List<string> livingNames = new List<string>();
    public static List<Text> playerNames = new List<Text>();

    [SerializeField] Text p1Name;
    [SerializeField] Text p2Name;
    [SerializeField] Text p3Name;
    [SerializeField] Text p4Name;

    static List<GameObject> livingPlayers = new List<GameObject>();
    static List<GameObject> currentPlayers = new List<GameObject>();

    public GameObject wincanvas;
    public Text winnerName;

    [SerializeField] Text timerText;
    [SerializeField] Button restartButton;
    static float timeStart = 90f;
    static float time;
    static bool matchStarted;
    [SerializeField] private InputSystem inputSystem;
    private int players = 0;
    public static Action onGameRestart;

    private void Awake()
    {
        time = timeStart;
        matchStarted = false;
        //LobbyPlayer.OnGameStart += (a) => matchStarted = true;
        PlayerNetworkConfig.playerLoaded += FreePlayers;

        inputSystem = FindObjectOfType<InputSystem>();

        playerNames.Add(p1Name);
        playerNames.Add(p2Name);
        playerNames.Add(p3Name);
        playerNames.Add(p4Name);

        p1Name.gameObject.SetActive(false);
        p2Name.gameObject.SetActive(false);
        p3Name.gameObject.SetActive(false);
        p4Name.gameObject.SetActive(false);
    }

    public static void AddPlayer(GameObject player)
    {
        livingPlayers.Add(player);
        livingNames.Add(player.name);
        currentNames.Add(player.name);
        currentPlayers.Add(player);
    }

    public static void RemoveDisconectedPlayer(GameObject player)
    {
        int aux = livingNames.IndexOf(player.name);
        livingNames[aux] = null;
        currentNames[aux] = null;
        livingPlayers.Remove(player);
        currentPlayers.Remove(player);
    }

    public static void RemoveDeadPlayer(GameObject player)
    {
        livingPlayers.Remove(player);
        int aux = livingNames.IndexOf(player.name);
        livingNames[aux] = null;
    }

    public static void VictoryCondition(string player)
    {
        Debug.Log($"Ha ganado {player}");
    }

    public void Start()
    {
        // winnerName = wincanvas.GetComponent<Text>();
        wincanvas.SetActive(false);
    }

    public static string EscogerGanador()
    {
        int aux = 0;
        GameObject ganador = null;
        foreach (GameObject go in livingPlayers)
        {
            if (go.GetComponent<FighterMovement>().health.Value >= aux)
            {
                aux = (int)go.GetComponent<FighterMovement>().health.Value;
                ganador = go;
            }
        }
        return ganador.name;
    }

    public void Update()
    {
        if (IsHost && currentPlayers.Count > 1) { restartButton.interactable = true; }
        else { restartButton.interactable = false; }
        
        if (!IsServer || !matchStarted) return;
        time -= Time.deltaTime;

        UpdateTimerClientRpc(time);

        for (int i = 0; i < livingNames.Count; i++)
        {
            string name = livingNames[i];
            SetNamesClientRpc(i, name);
        }

        if (time <= timeStart - 10)
        {
            if (livingPlayers.Count == 1 || time <= 0)
            {
                string player;
                matchStarted = false;
                if (livingPlayers.Count == 1)
                {
                    player = livingPlayers[0].name;
                }
                else
                {
                    player = EscogerGanador();
                }
                WinClientRpc(player);
            }
        }
    }

    public void FreePlayers()
    {
        players++;
        if(players == LobbyUI.playerCount.Value) { 
            EnableInputSystemClientRpc();
            matchStarted = true;
        }
    }

    [ClientRpc]
    public void DisableInputSystemClientRpc()
    {
        inputSystem.DisableInputs();
    }

    [ClientRpc]
    public void EnableInputSystemClientRpc()
    {
        inputSystem.EnableInputs();
    }

    public void OnGameRestart()
    {
        time = timeStart;
        onGameRestart?.Invoke();
        matchStarted = false;
        livingPlayers = new List<GameObject>();
        foreach (GameObject player in currentPlayers) 
        { 
            livingPlayers.Add(player);
        }
        livingNames = new List<string>();
        foreach (string name in currentNames)
        {
            livingNames.Add(name);
        }
        //currentPlayers = new List<GameObject>();
        RestartClientRpc();
    }

    [ClientRpc]
    public void RestartClientRpc()
    {
        EnableInputSystemClientRpc();
        wincanvas.SetActive(false);
        foreach (GameObject player in currentPlayers)
        {
            Debug.Log("Jugadores conectados" + player.name);
        }
        foreach (GameObject player in livingPlayers)
        {
            Debug.Log("Jugadores vivos " + player.name);
        }
    }

    [ClientRpc]
    public void SetNamesClientRpc(int i, string name)
    {
        if (name != null)
        {
            playerNames[i].gameObject.SetActive(true);
            playerNames[i].text = name;
        }
        else
        {
            playerNames[i].gameObject.SetActive(false);
        }
    }

    [ClientRpc]
    public void WinClientRpc(string win)
    {
        VictoryCondition(win);
        string winner = win;
        winnerName.text = winner;
        wincanvas.SetActive(true);
        DisableInputSystemClientRpc();
    }

    [ClientRpc]
    public void UpdateTimerClientRpc(float timeRemaining)
    {
        if (timeRemaining <= 0)
        {
            timeRemaining = 0;
        }
        timerText.text = timeRemaining.ToString("0.0");
    }
}
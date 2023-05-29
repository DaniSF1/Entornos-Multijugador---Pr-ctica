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
    public static List<string> currentNames = new List<string>();       //Creamos una lista para los nombres de los jugadores que se encuentran en la partida
    public static List<string> livingNames = new List<string>();        //Creamos otra lista para los nombres de los jugadores que estan vivos y jugando.
    public static List<Text> playerNames = new List<Text>();            //Lista auxiliar para manejar las posiciones de los nombres de los jugadores en la interfaz

    [SerializeField] Text p1Name;
    [SerializeField] Text p2Name;
    [SerializeField] Text p3Name;
    [SerializeField] Text p4Name;                                       //Cuadros de texto de la interfaz. Contienen los nombres de los jugadores.

    static List<GameObject> livingPlayers = new List<GameObject>();     //Creamos una lista para los jugadores que estan vivos
    static List<GameObject> currentPlayers = new List<GameObject>();    //Creamos una lista para los jugadores que estan conectados

    public GameObject wincanvas;                                        //Canvas de la pantalla de victoria
    public Text winnerName;                                             //Texto que contiene el nombre del ganador

    [SerializeField] Text timerText;                                    //Texto que contiene el tiempo restante de la partida
    [SerializeField] Button restartButton;                              //Boton para reiniciar la partida
    static float timeStart = 90f;                                       //Tiempo inicial de la partida
    static float time;                                                  //Tiempo actual de la partida
    static bool matchStarted;                                           //Booleano que nos indica si hemos empezado o no la partida
    [SerializeField] private InputSystem inputSystem;                   //InputSystem para controlar el comienzo sincronizado
    private int players = 0;                                            //Jugadores instanciados actuales. Nos sirve para sincronizar el comienzo
    public static Action onGameRestart;                                 //Action usado para reiniciar la partida.

    private void Awake()
    {
        time = timeStart;                                               //Establecemos el tiempo inicial.
        matchStarted = false;                                           //Establecemos que la partida no ha empezado hasta que se indique lo contrario
        //LobbyPlayer.OnGameStart += (a) => matchStarted = true;
        PlayerNetworkConfig.playerLoaded += FreePlayers;                //Cuando cargamos un jugador, llamamos al metodo FreePlayer, usado para el comienzo sincronizado

        inputSystem = FindObjectOfType<InputSystem>();                  //Tomamos el InputSystem que tiene la escena

        playerNames.Add(p1Name);
        playerNames.Add(p2Name);
        playerNames.Add(p3Name);
        playerNames.Add(p4Name);                                        //Añadimos a la lista auxiliar de textos los distintos cuadros de texto para la interfaz.

        p1Name.gameObject.SetActive(false);
        p2Name.gameObject.SetActive(false);
        p3Name.gameObject.SetActive(false);
        p4Name.gameObject.SetActive(false);                             //Desactivamos dichos campos de texto.
    }

    public static void AddPlayer(GameObject player)                     //Funcion llamada cuando se crea un nuevo jugador en PlayerNetworkConfig
    {
        livingPlayers.Add(player);                                      //Lo añadimos a la lista de jugadores vivos
        livingNames.Add(player.name);                                   //Añadimos su nombre a la lista de personajes vivos
        currentNames.Add(player.name);                                  //Añadimos su nombre a la lista de jugadores actuales
        currentPlayers.Add(player);                                     //Añadimos al jugador a la lista de jugadores conectados.
    }

    public static void RemoveDisconectedPlayer(GameObject player)       //Funcion llamada cuando se desconecta un jugador. Concretamente se llama en PlayerNetworkConfig
    {
        int aux = livingNames.IndexOf(player.name);                     //Guardamos en un auxiliar la posicion del nombre
        livingNames[aux] = null;                                        
        currentNames[aux] = null;                                       //Establecemos ambos nombres a null para gestionarlos posteriormente
        livingPlayers.Remove(player);                   
        currentPlayers.Remove(player);                                  //Lo sacamos de ambas listas. El jugador desconectado no podra acceder a la partida en caso de que se reinicie
    }

    public static void RemoveDeadPlayer(GameObject player)              //Funcion llamada cuando muere un jugador. Se llama en FighterMovement cuando el jugador muere
    {
        livingPlayers.Remove(player);                                   //Sacamos al jugador de la lista de personajes vivos
        int aux = livingNames.IndexOf(player.name);                     
        livingNames[aux] = null;                                        //Establecemos su nombre a null
    }

    public static void VictoryCondition(string player)                  //Condicion de victoria. Se usa solo para hacer depuracion
    {
        Debug.Log($"Ha ganado {player}");
    }

    public void Start()
    {
        // winnerName = wincanvas.GetComponent<Text>();
        wincanvas.SetActive(false);                                     //Desactivamos el canvas que contiene la pantalla de victoria en el start del metodo.
    }

    public static string EscogerGanador()                               //Funcion que escoge el ganador en caso de que queden jugadores vivos
    {
        int aux = 0;
        GameObject ganador = null;
        foreach (GameObject go in livingPlayers)                        //Recorremos la lista de jugadores
        {
            if (go.GetComponent<FighterMovement>().health.Value >= aux)     //Tomamos la vida del jugador y vemos si es mayor que la vida del anterior jugador
            {
                aux = (int)go.GetComponent<FighterMovement>().health.Value; //Actualizamos el auxiliar para denotar que este jugador tiene mas vida que el anterior
                ganador = go;                                               //Establecemos quien es el ganador
            }
        }
        return ganador.name;                                                //Devolvemos el nombre del ganador
    }

    public void Update()
    {
        if (IsHost && currentPlayers.Count > 1) { restartButton.interactable = true; }  //Solo el host puede reiniciar la partida en caso de que quede mas de un jugador.
        else { restartButton.interactable = false; }                        //Hacemos no interactuable el boton de reestart en caso de que no seas servidor o no haya mas de un jugador
        
        if (!IsServer || !matchStarted) return;                             //Si la partida no ha empezado, no actualizamos nada. Todos los calculos se hacen en el servidor
        time -= Time.deltaTime;                             //Actualizamos el contador del tiempo de la partida de forma constante.

        UpdateTimerClientRpc(time);                         //Actualizamos en los clientes el contador del tiempo para que se vea correctamente.

        for (int i = 0; i < livingNames.Count; i++)         //De los jugadores que estan vivos...
        {
            string name = livingNames[i];
            SetNamesClientRpc(i, name);                     //Sacamos sus nombres por pantalla
        }

        if (time <= timeStart - 10)                         //Pasados 10 segundos de partida...
        {
            if (livingPlayers.Count == 1 || time <= 0)      //En caso de que solo quede un jugador vivo o se acabe el tiempo
            {
                string player;
                matchStarted = false;                       //Paramos la partida (el update de GameManager)
                if (livingPlayers.Count == 1)               //Si solo queda un jugador vivo
                {
                    player = livingPlayers[0].name;         //Tomamos su nombre
                }
                else
                {
                    player = EscogerGanador();              //Si no, se llama a la funcion anteriormente comentada
                }
                WinClientRpc(player);                       //Enviamos a los clientes el nombre del jugador y los saca por pantalla a todos los clientes
            }
        }
    }

    public void FreePlayers()                               //Funcion que libera el input system de los jugadores. Se llama cada vez que se carga un jugador en PlayerNetworkConfig
    {
        players++;                                          //Añadimos un jugador al contador
        if(players == LobbyUI.playerCount.Value) {          //Cuando esten todos los jugadores listos...
            EnableInputSystemClientRpc();                   //Desbloqueamos el input system en los clientes
            matchStarted = true;                            //Establecemos que empiece la partida
        }
    }

    [ClientRpc]
    public void DisableInputSystemClientRpc()               //Funcion que desactiva el inputSystem en los clientes
    {
        inputSystem.DisableInputs();
    }

    [ClientRpc]
    public void EnableInputSystemClientRpc()                //Funcion que activa el inputSystem en los clientes
    {
        inputSystem.EnableInputs();
    }

    public void OnGameRestart()                             //Cuando se reinicia el juego...
    { 
        time = timeStart;                                   //Reestablecemos el tiempo al inicial
        onGameRestart?.Invoke();                            //Invoca los metodos asociados a onGameRestart. Es decir, invoca los metodos encargados de reiniciar la partida
        matchStarted = true;                                //Establecemos que la partida ha empezado
        livingPlayers = new List<GameObject>();             //Creamos de nuevo la lista de jugadores vivos
        foreach (GameObject player in currentPlayers) 
        { 
            livingPlayers.Add(player);                      //Añadimos solo los jugadores que siguen conectados
        }
        livingNames = new List<string>();                   //Hacemos lo mismo con los nombres
        foreach (string name in currentNames)
        {
            livingNames.Add(name);
        }
        RestartClientRpc();                                 //Hacemos que en los clientes se reinicie tambien el juego
    }

    [ClientRpc]
    public void RestartClientRpc()                          
    {
        EnableInputSystemClientRpc();                       //Volvemos a activar el InputSystem para los clientes. Permite que estos se muevan
        wincanvas.SetActive(false);                         //Desactivamos el canvas que muestra al ganador
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
    public void SetNamesClientRpc(int i, string name)       //Establecemos en la interfaz de los clientes los nombres de los jugadores en su posicion concreta
    {
        if (name != null)
        {
            playerNames[i].gameObject.SetActive(true);      //Si el nombre no es nulo, activamos la caja de texto y
            playerNames[i].text = name;                     //ponemos el nombre del jugador en la misma
        }
        else
        {
            playerNames[i].gameObject.SetActive(false);     //Si no, desactivamos la caja de texto
        }
    }

    [ClientRpc]
    public void WinClientRpc(string win)                    //Funcion que informa a los clientes del ganador
    {
        string winner = win;                                //Tomamos el nombre del ganador
        winnerName.text = winner;                           //Establecemos en el texto del canvas el nombre del ganador
        wincanvas.SetActive(true);                          //Activamos dicho canvas
        DisableInputSystemClientRpc();                      //Desactivamos el inputSystem de los jugadores para finalizar la partida
    }

    [ClientRpc]
    public void UpdateTimerClientRpc(float timeRemaining)   //Actualizamos en el cliente el texto del contador
    {
        if (timeRemaining <= 0)
        {
            timeRemaining = 0;                              //Para no tener valores negativos, ponemos a 0 el contador
        }
        timerText.text = timeRemaining.ToString("0.0");     //Sacamos con un decimal el tiempo restante en los clientes.
    }
}
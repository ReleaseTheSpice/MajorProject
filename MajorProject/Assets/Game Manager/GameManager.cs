using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using ExitGames.Client.Photon.StructWrapping;
using UnityEngine;
using UnityEngine.SceneManagement;

using Photon.Pun;
using Photon.Realtime;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviourPunCallbacks
{
    #region Public Fields

    public GameObject playerPrefab;
    public GameObject stackPrefab;
    public GameObject cardPrefab;
    // Network manager prefab to handle things synchronously
    public GameObject networkManagerObj; 
    
    // "Static" makes it so you can simply do GameManager.Instance.xxx() from anywhere in your code
    // Reuse this later??
    public static GameManager Instance;
    // Reference to this instance's NetworkManager script
    public static NetworkManager NetworkManager;
    
    #endregion
    
    #region Private Fields
    
    
    #endregion
    
    #region Photon Callbacks

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }
    
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.LogFormat("OnPlayerEnteredRoom() {0}", newPlayer.NickName); // not seen if you're the player connecting

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom

            LoadArena();
        }
    }
    
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        Debug.LogFormat("OnPlayerLeftRoom() {0}", otherPlayer.NickName); // seen when other disconnects

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom

            LoadArena();
        }
    }

    #endregion

    #region MonoBehaviour Callbacks

    private void Start()
    {
        
        if (networkManagerObj != null && NetworkManager == null)
        {
            GameObject netMan = PhotonNetwork.Instantiate(networkManagerObj.name, Vector3.zero, Quaternion.identity, 0);
            NetworkManager = netMan.GetComponent<NetworkManager>();
        }
        else
        {
            Debug.LogError("Missing networkManager Reference. Please set it up in GameObject 'Game Manager'", this);
        }

        Instance = this;
        
        if (playerPrefab == null)
        {
            Debug.LogError("Missing playerPrefab Reference. Please set it up in GameObject 'Game Manager'", this);
        }
        else
        {
            // If the local player for this client is not instantiated, instantiate it
            if (Player.LocalPlayerInstance == null)
            {
                // Instantiate the Player
                Debug.LogFormat("We are Instantiating LocalPlayer from {0}", SceneManagerHelper.ActiveSceneName);
                // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
                // pack up deck code for instantiated player into custom init data
                List<int> deckCode = new List<int>() {0, 1, 2};
                object[] customInitData = new[] { deckCode };
                
                GameObject p = PhotonNetwork.Instantiate(
                    this.playerPrefab.name, 
                    new Vector3(0f, 0f, 0f), 
                    Quaternion.identity, 
                    0, 
                    customInitData);
                // RPC calls are only sent to other instances of the same prefab
                NetworkManager.PV.RPC("SetPlayerName", RpcTarget.AllBuffered, 
                    p.GetComponent<PhotonView>().ViewID, PhotonNetwork.NickName);
                // Add the local player's ID to the NetworkManager's list of players
                AddPlayerID(p.GetComponent<PhotonView>().ViewID);
                // Set the local player as the NetworkManager's local player
                NetworkManager.LocalPlayer = p;

                // Instantiate a Stack for the local player
                GameObject s = PhotonNetwork.Instantiate(
                    this.stackPrefab.name, 
                    new Vector3(0f, 230f, 0f), 
                    Quaternion.identity, 
                    0);
                s.GetComponent<Stack>().owner = p.GetComponent<Player>();
                s.transform.position = GetNewStackPosition(NetworkManager.PV.ViewID);
                NetworkManager.PV.RPC("SetStackOwner", RpcTarget.AllBuffered, 
                    s.GetComponent<PhotonView>().ViewID, p.GetComponent<PhotonView>().ViewID);

            }
            else
            {
                Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
            }
        }
    }

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this) 
        { 
            Destroy(this); 
        } 
        else 
        { 
            Instance = this; 
        } 
        DontDestroyOnLoad(this);
    }

    #endregion
    
    #region Public Methods

    // Wrapping the photon LeaveRoom in a public method for abstraction
    public void LeaveRoom()
    {
        // Make the local player leave the Photon Network room
        PhotonNetwork.LeaveRoom();
    }

    #endregion

    #region Private Methods

    void LoadArena()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogError("PhotonNetwork: Trying to Load a level but we are not the master Client");
            return;
        }
        Debug.LogFormat("PhotonNetwork: Loading Level: {0}", PhotonNetwork.CurrentRoom.PlayerCount);
        //PhotonNetwork.LoadLevel("Room for " + PhotonNetwork.CurrentRoom.PlayerCount);
        PhotonNetwork.LoadLevel("Room for 2");
    }

    private Vector3 GetNewStackPosition(int ID)
    {
        int identifier = (int)Math.Floor(ID / 1000f) - 1;

        float x;
        if (identifier%2 == 0)
        {
            x = (float)Math.Ceiling(identifier/2f) * 400f;
        }
        else
        {
            x = (float)Math.Ceiling(identifier/2f) * -400f;
        }
        return new Vector3(x, 230f, 0f);

        // switch (identifier)
        // {
        //     case 1:
        //         return new Vector3(0f, 230f, 0f);
        //     case 2:
        //         return new Vector3(-400f, 230f, 0f);
        //     case 3:
        //         return new Vector3(400f, 230f, 0f);
        // }
        // return new Vector3(0f, 230f, 0f);
    }

    private void AddPlayerID(int ID)
    {
        byte eventCode = 1;
        object[] content = { ID };
        // You would have to set the Receivers to All in order to receive this event on the local client as well
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.All, 
            CachingOption = EventCaching.AddToRoomCache
        };
        PhotonNetwork.RaiseEvent(eventCode, content, raiseEventOptions, SendOptions.SendReliable);
    }
    
    #endregion
}

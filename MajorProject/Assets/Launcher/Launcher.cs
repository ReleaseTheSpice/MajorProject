using System;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Launcher : MonoBehaviourPunCallbacks
{
    #region Private Serializable Fields
    
    [Tooltip("The UI panel to let the user see name, connect, and play")]
    [SerializeField]
    private GameObject controlPanel;
    
    [Tooltip("The UI Label to inform the user that the connection is in progress")]
    [SerializeField]
    private GameObject progressLabel;
    
    [Tooltip("The UI Button to cancel the connection progress")]
    [SerializeField]
    private GameObject cancelButton;
    

    #endregion
    
    [HideInInspector]
    public byte maxPlayersPerRoom = 4;
    
    #region Private Fields
    
    /// <summary>
    /// Keep track of the current process. Since connection is asynchronous and is based on several callbacks from Photon,
    /// we need to keep track of this to properly adjust the behavior when we receive call back by Photon.
    /// Typically this is used for the OnConnectedToMaster() callback.
    /// </summary>
    bool isConnecting;

    private bool waitingForPlayers;
    
    // This client's version number. Users are separated from each other by gameVersion (which allows me to make breaking changes).
    // Should be left at 1 until changes need to be made to a live game
    string gameVersion = "1";

    #endregion
    
    #region MonoBehaviour CallBacks
    
    void Awake()
    {
        // #Critical
        // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        // Set the progress label to be inactive and show the default control panel
        progressLabel.SetActive(false);
        controlPanel.SetActive(true);
        cancelButton.SetActive(false);
    }

    #endregion
    
    #region Public Methods
    
    // Start the connection process.
    // - If already connected, we attempt joining a random room
    // - if not yet connected, Connect this application instance to Photon Cloud Network
    public void Connect()
    {
        // Sow "Connecting..." progress label and disable the control panel
        progressLabel.SetActive(true);
        controlPanel.SetActive(false);
        cancelButton.SetActive(true);
        
        // we check if we are connected or not, we join if we are, else we initiate the connection to the server.
        if (PhotonNetwork.IsConnected)
        {
            // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnJoinRandomFailed() and we'll create one.
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            // keep track of the will to join a room, because when we come back from the game we will get a callback that we are connected, so we need to know what to do then
            isConnecting = PhotonNetwork.ConnectUsingSettings();
            
            // #Critical, we must first and foremost connect to Photon Online Server.
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = gameVersion;
        }
    }
    
    public void Cancel()
    {
        // Show the default control panel again
        progressLabel.SetActive(false);
        controlPanel.SetActive(true);
        cancelButton.SetActive(false);
        
        // Disconnect from the server
        PhotonNetwork.Disconnect();
    }
    
    #endregion
    
    #region MonoBehaviourPunCallbacks Callbacks
    
    public override void OnConnectedToMaster()
    {
        Debug.Log("PUN Basics Tutorial/Launcher: OnConnectedToMaster() was called by PUN");

        // we don't want to do anything if we are not attempting to join a room.
        // this case where isConnecting is false is typically when you lost or quit the game,
        // when this level is loaded, OnConnectedToMaster will be called, so we won't want to do anything.
        if (isConnecting)
        {
            // #Critical: First we try to join a potential existing room. If there aren't any, we'll be called back with OnJoinRandomFailed()
            PhotonNetwork.JoinRandomRoom();
            isConnecting = false;
        }
    }
    
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarningFormat("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {0}", cause);
        
        // Show the default control panel again
        progressLabel.SetActive(false);
        controlPanel.SetActive(true);
    }
    
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("PUN Basics Tutorial/Launcher: OnJoinRandomFailed() was called by PUN. " +
                  "No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");
    
        // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayersPerRoom });
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("PUN Basics Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
        // #Critical: We only load if we are the first player, else we rely on `PhotonNetwork.AutomaticallySyncScene` to sync our instance scene.
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            //TODO: Add another button to the launcher scene to specify and pass in lobby size,
            //TODO: then wait for that many players to join before loading the room

            // #Critical - Load the Room Level.
            PhotonNetwork.LoadLevel("Game");
        }
        // else
        // {
        //     waitingForPlayers = true;
        // }
    }
    
    #endregion
}
    
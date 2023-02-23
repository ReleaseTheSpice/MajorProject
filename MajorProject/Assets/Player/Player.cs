using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using TMPro;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Player : MonoBehaviour, IPunInstantiateMagicCallback
{
    #region Public Fields

    public GameObject playerCanvasPrefab;
    public GameObject myDeck;
    public GameObject myHand;
    
    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalPlayerInstance;
    
    public int life;
    public string playerName;

    public int turnCounter = 99;
    
    #endregion

    private List<int> deckCode;

    #region MohoBehaviour Callback Handlers

    // Start is called before the first frame update
    void Start()
    {
        life = 20;
        // Only instantiate the canvas if this is the local player
        if (PhotonView.Get(this).IsMine)
        {
            // Instantiate the canvas
            if (playerCanvasPrefab == null)
            {
                Debug.LogError("Player Canvas prefab reference is null", this);
            }
            else
            {
                // Instantiating locally so other players can't see it
                GameObject myCanvas = Instantiate(playerCanvasPrefab);
                // Connect the canvas to the player
                myCanvas.GetComponent<Canvas>().worldCamera = Camera.main;
                myHand = myCanvas.transform.Find("Hand").gameObject;
                myDeck = myCanvas.transform.Find("Deck").gameObject;
                myDeck.GetComponent<Deck>().player = gameObject;
            }
            
            List<int> deckCode = new List<int>(){ 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2 };
            myDeck.GetComponent<Deck>().GenerateDeck(deckCode);
            // Set turn order
            turnCounter = (int)Math.Floor(PhotonView.Get(this).ViewID / 1000f) - 1;
            if (turnCounter == 0)
            {
                Debug.Log(PhotonNetwork.NickName + " will go first");
                StartTurn();
            }
        }
    }

    private void Awake()
    {
        // #Important
        // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
        if (PhotonView.Get(this).IsMine)
        {
            Player.LocalPlayerInstance = this.gameObject;
        }
        // #Critical
        // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
        DontDestroyOnLoad(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (life <= 0)
        {
            Debug.Log("You lose!");
            GameManager.Instance.LeaveRoom();
        }
    }
    
    #endregion

    #region Photon Callbacks

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] instantiationData = info.photonView.InstantiationData;
        deckCode = instantiationData[0] as List<int>;
    }
    
    #endregion
    
    #region Public Methods

    
    [PunRPC]
    public void ChangeTurn()
    {
        ChangeTurnCounter();
    }

    public void ChangeTurnCounter()
    {
        turnCounter--;
        if (turnCounter < 0)
        {
            turnCounter = PhotonNetwork.CurrentRoom.PlayerCount - 1;
        }
        Debug.Log(PhotonNetwork.NickName + ": " + turnCounter);
        //playerName = playerName + " " + turnCounter;
        if (turnCounter == 0)
        {
            Debug.Log(PhotonNetwork.NickName + " starting turn");
            StartTurn();
        }
    }
    
    public void EndTurn()
    {
        Debug.Log(PhotonNetwork.NickName + " Ending turn");
        // Set each card in hand to inactive
        foreach (GameObject card in myHand.GetComponent<Hand>().cards)
        {
            card.GetComponent<Interactable>().myTurn = false;
        }
        
        // Trigger event to change turn
        object[] data = new object[] { };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(2, data, raiseEventOptions, SendOptions.SendReliable);
    }
    
    #endregion
    
    #region Private Methods

    private void StartTurn()
    {
        Debug.LogError(PhotonNetwork.NickName + " Starting turn");
        // Draw cards
        myDeck.GetComponent<Deck>().DrawCard(2);

        // Set each card in hand to active
        foreach (GameObject card in myHand.GetComponent<Hand>().cards)
        {
            card.GetComponent<Interactable>().myTurn = true;
        }
    }

    #endregion
}

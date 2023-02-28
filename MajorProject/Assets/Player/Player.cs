using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using TMPro;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class Player : MonoBehaviour, IPunInstantiateMagicCallback
{
    #region Public Fields

    public GameObject playerCanvasPrefab;
    public GameObject myDeck;
    public GameObject myHand;

    public GameObject myStack;
    
    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalPlayerInstance;
    
    public int life;
    public string playerName;

    public int turnCounter = 99;
    public bool passedTurn = true;
    
    #endregion
    
    #region Private Fields

    private List<int> deckCode;
    private GameObject passButton;

    #endregion


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

                passButton = myHand.transform.GetChild(0).gameObject;
                passButton.GetComponent<Button>().onClick.AddListener(PassTurn);
                passButton.SetActive(false);
            }
            
            //TODO: Revisit this method if deck creation becomes a feature
            List<int> deckCode = new List<int>(){ 0, 1, 2, 3, 4, 5, 6, 7, 8, 0, 1, 3 };
            myDeck.GetComponent<Deck>().GenerateDeck(deckCode);
            
            // Draw starting hand
            myDeck.GetComponent<Deck>().DrawCard(5);
            
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

    //TODO: Revisit this method if deck creation becomes a feature
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        // Debug.Log("Instantiating player");
        // object[] instantiationData = info.photonView.InstantiationData;
        // //deckCode = instantiationData[0] as List<int>;
        //
        // // Build the deck code from the instantiation data
        // foreach (int cardCode in instantiationData)
        // {
        //     //TODO: Deck code is null when this is called?
        //     // that probably means "this" isnt even initialized yet..
        //     deckCode.Add(cardCode);
        // }
    }
    
    #endregion
    
    #region Public Methods

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
            if (passedTurn)
            {
                // Resolve all effects
                GameManager.NetworkManager.PV.RPC("ResolveStacks", RpcTarget.All);
                GameManager.NetworkManager.PV.RPC(
                    "UpdateStackGlow", 
                    RpcTarget.All, 
                    myStack.GetComponent<PhotonView>().ViewID, 
                    0);
                passedTurn = false;
                
                // Draw cards
                myDeck.GetComponent<Deck>().DrawCard(2);
            }

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
        
        // Disable the glow on the stack
        GameManager.NetworkManager.PV.RPC(
            "UpdateStackGlow", 
            RpcTarget.All, 
            myStack.GetComponent<PhotonView>().ViewID, 
            0);
        // Set the pass button to inactive
        passButton.SetActive(false);
        
        // Trigger event to change turn
        object[] data = new object[] { };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(2, data, raiseEventOptions, SendOptions.SendReliable);
    }
    
    #endregion
    
    #region Private Methods

    private void StartTurn()
    {
        //Debug.LogError(PhotonNetwork.NickName + " Starting turn");

        // Set each card in hand to active
        foreach (GameObject card in myHand.GetComponent<Hand>().cards)
        {
            card.GetComponent<Interactable>().myTurn = true;
        }
        
        // Enable the glow on the stack
        GameManager.NetworkManager.PV.RPC(
            "UpdateStackGlow", 
            RpcTarget.All, 
            myStack.GetComponent<PhotonView>().ViewID, 
            1);
        // Set the pass button to active
        passButton.SetActive(true);
    }

    private void PassTurn()
    {
        passedTurn = true;
        // End turn
        EndTurn();
        // Enable the glow on the stack
        GameManager.NetworkManager.PV.RPC(
            "UpdateStackGlow", 
            RpcTarget.All, 
            myStack.GetComponent<PhotonView>().ViewID, 
            2);
    }
    
    #endregion
}

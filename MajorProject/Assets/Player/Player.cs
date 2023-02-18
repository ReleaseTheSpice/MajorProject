using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Photon.Pun;

public class Player : MonoBehaviour
{
    #region Public Fields

    public GameObject myDeck;
    public GameObject myHand;
    
    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalPlayerInstance;
    
    public int life;
    public string playerName;

    public int turnCounter = 99;
    
    #endregion

    #region MohoBehaviour Callback Handlers

    // Start is called before the first frame update
    void Start()
    {
        life = 20;
        
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

        //lifeText.GetComponent<TextMeshProUGUI>().text = life.ToString();
    }
    
    #endregion

    #region Public Methods

    
    [PunRPC]
    public void ChangeTurn()
    {
        turnCounter--;
        if (turnCounter < 0)
        {
            turnCounter = PhotonNetwork.CurrentRoom.PlayerCount - 1;
        }
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
        
        // Change turn counter
        PhotonView.Get(this).RPC("ChangeTurn", RpcTarget.All);
    }
    
    #endregion
    
    #region Private Methods

    private void StartTurn()
    {
        // Draw a card
        myDeck.GetComponent<Deck>().DrawCard(2);
        // Set turn counter to 0
        turnCounter = 0;
        
        // Set each card in hand to active
        foreach (GameObject card in myHand.GetComponent<Hand>().cards)
        {
            card.GetComponent<Interactable>().myTurn = true;
        }
    }

    #endregion
}

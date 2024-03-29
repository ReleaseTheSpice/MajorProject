using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviour, IOnEventCallback
{
    #region Public Fields

    public static NetworkManager Instance;
    public PhotonView PV;
    
    // Keep track of the players in the game
    public static List<int> playerIDs = new List<int>();
    
    // Keep reference to local player
    public static GameObject LocalPlayer;

    #endregion
    
    #region MonoBehaviour Callbacks

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        PV = PhotonView.Get(this);
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    
    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    #endregion

    #region Photon Callbacks

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == 1)
        {
            object[] data = (object[]) photonEvent.CustomData;
            int playerID = (int) data[0];
            playerIDs.Add(playerID);
        }
        
        if (photonEvent.Code == 2)
        {
            if (LocalPlayer != null && PV.IsMine)
            {
                LocalPlayer.GetComponent<Player>().ChangeTurnCounter();
            }
            else
            {
                //Debug.LogError("Local player is null");
            }
        }
    }

    #endregion
    
    #region Public Methods
    
    [PunRPC]
    public void SetPlayerName(int viewID, string name)
    {
        PhotonView.Find(viewID).GetComponent<Player>().playerName = name;
    }
    
    [PunRPC]
    public void SetStackOwner(int viewID, int ownerID)
    {
        PhotonView.Find(viewID).GetComponent<Stack>().owner = PhotonView.Find(ownerID).GetComponent<Player>();
    }
    
    [PunRPC]
    public void SetPlayerLife(int viewID, int life)
    {
        PhotonView.Find(viewID).GetComponent<Player>().life = life;
    }
    
    [PunRPC]
    public void AddCardToStack(int cardViewID, int stackID, int playerID, string cardName)
    {
        GameObject card = PhotonView.Find(cardViewID).gameObject;
        GameObject stack = PhotonView.Find(stackID).gameObject;
        // Rename the card to not include (Clone)
        card.name = cardName;
        // Set the card owner
        card.GetComponent<Card>().owner = PhotonView.Find(playerID).gameObject;
        // Put it on the stack canvas
        card.GetComponent<Interactable>().updateCanvas(
            stack.GetComponentInChildren<Canvas>());
        // Set the transform parent
        card.transform.SetParent(
            stack.GetComponent<Stack>().GetCanvasTransform());
        // Disable the raycaster on the image so the card can't be dragged
        // (raycastTarget is already disabled on other components of the card)
        card.GetComponent<Image>().raycastTarget = false;
        // Call the stack's AddCard method
        stack.GetComponent<Stack>().AddCard(card);
        card.GetComponent<Card>().AddEffect(stack.GetComponent<Stack>());
    }
    
    [PunRPC]
    public void ClearStack(int stackID)
    {
        PhotonView.Find(stackID).GetComponent<Stack>().cards.Clear();
        PhotonView.Find(stackID).GetComponent<Stack>().cardEffects.Clear();
    }

    [PunRPC]
    public void NextRound()
    {
        // Resolve all stacks, reset passed turns, draw cards
        // Get each client to resolve their stacks
        GameObject stack = LocalPlayer.GetComponent<Player>().myStack;
        stack.GetComponent<Stack>().ResolveEffects();
        // Clear stack glow (WHY DOESNT THIS WORK)
        stack.transform.GetChild(0).gameObject.SetActive(false);
        
        // Reset passed turns
        LocalPlayer.GetComponent<Player>().passedTurn = false;
        
        // Draw cards
        GameObject deck = LocalPlayer.GetComponent<Player>().myDeck;
        deck.GetComponent<Deck>().DrawCard(2);
    }

    [PunRPC]
    public void UpdateStackGlow(int stackID, int type)
    {
        Transform stack = PhotonView.Find(stackID).gameObject.transform;
        switch (type)
        {
            case 0: // Disable glow
                stack.GetChild(0).gameObject.SetActive(false);
                stack.GetChild(1).gameObject.SetActive(false);
                break;
            case 1: // Current turn glow
                stack.GetChild(1).gameObject.SetActive(true);
                break;
            case 2: // Passed turn glow
                stack.GetChild(0).gameObject.SetActive(true);
                break;
        }
    }

    [PunRPC]
    public void DestroyCard(int cardViewID)
    {
        // Find a card with given Photon ID and destroy it on all clients
        PhotonNetwork.Destroy(PhotonView.Find(cardViewID).gameObject);
    }
    
    [PunRPC]
    public void CheckForWinner()
    {
        // Check if the lobby size is 1, if so show the popup and destroy the game
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            // Show popup
            GameObject popup = GameObject.Find("YouWin");
            popup.SetActive(true);
            // Destroy the game
            StartCoroutine(DelayedDestroy(5));
        }
    }
    
    IEnumerator DelayedDestroy(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        PhotonNetwork.DestroyAll();
    }

    #endregion
}

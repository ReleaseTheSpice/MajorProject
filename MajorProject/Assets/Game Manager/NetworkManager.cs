using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviour
{
    #region Public Fields

    public static NetworkManager Instance;
    public PhotonView PV;

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

    // Update is called once per frame
    void Update()
    {
        
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

    #endregion
}

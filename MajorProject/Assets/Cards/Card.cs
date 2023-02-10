using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class Card : MonoBehaviour, IPunObservable
{
    [Header("Card Details")]
    public string cardName;
    public int cardID;
    public string cardDescription;

    [Header("References")] 
    public GameObject owner;
    public GameObject cardNameText;
    public GameObject cardDescriptionText;
    
    // PhotonView ID of the stack that this card belongs to, 0 if not on a stack
    public int stackID = 0;

    //TODO: REMOVE THIS AND IPUNOBSERVABLE?
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // if (stream.IsWriting)
        // {
        //     // We own this instance: send the others our data
        //     stream.SendNext(owner.GetComponent<PhotonView>().ViewID);
        // }
        // else
        // {
        //     // Networked instance, receive data
        //     int ownerID = (Int32) stream.ReceiveNext();
        //     owner = PhotonView.Find(ownerID).gameObject;
        //     if (stackID)
        // }
    }
    
    public void AddEffect(Stack stack)
    {
        Delegate effect = Delegate.CreateDelegate(typeof(Action), stack, cardName);
        stack.cardEffects.Add(effect);
    }

    #region Callback Handlers
    
    // Start is called before the first frame update
    void Start()
    {
        cardNameText.GetComponent<TextMeshProUGUI>().text = cardName;
        cardDescriptionText.GetComponent<TextMeshProUGUI>().text = cardDescription;
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
    
}

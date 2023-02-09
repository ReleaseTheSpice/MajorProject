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

    //TODO: REMOVE THIS AND IPUNOBSERVABLE?
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            //stream.SendNext(transform.position);
        }
        else
        {
            // Network player, receive data
            //transform.position = (Vector3) stream.ReceiveNext();
        }
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

    // Update is called once per frame
    void Update()
    {
        
    }
    
    #endregion
    
}

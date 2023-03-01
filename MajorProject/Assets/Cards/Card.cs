using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class Card : MonoBehaviour
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

    public void AddEffect(Stack stack)
    {
        // Remove spaces from the card name to make it a valid function name
        string effectName = cardName.Replace(" ", "");
        Delegate effect = Delegate.CreateDelegate(typeof(Action), stack, effectName);
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

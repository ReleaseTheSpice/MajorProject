using System;
using System.Collections;
using System.Collections.Generic;
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

    public void AddEffect(Stack stack)
    {
        Delegate effect = Delegate.CreateDelegate(typeof(Action), stack, cardName);
        stack.cardEffects.Add(effect);
        // Delegate effect = Delegate.CreateDelegate(typeof(Action), stack, "doSeomthing");
        // public delegate void effect(Player player);
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

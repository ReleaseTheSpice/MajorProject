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
}

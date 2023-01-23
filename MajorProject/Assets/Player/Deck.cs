using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;
using UnityEngine.EventSystems;

public class Deck : MonoBehaviour, IPointerDownHandler
{
    public List<Card> cards = new List<Card>();
    
    [Header("References")]
    public Hand hand;

    public void Init(int i)
    {
        Debug.Log(i);
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("OnPointerDown");
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void GenerateDeck(List<int> cardIds)
    {
        foreach (int cardId in cardIds)
        {
            // Card card = new Card(cardId);
            // cards.Add(card);
        }
    }
    
    // Add a card to the bottom of the deck
    public void AddCard(Card card)
    {
        cards.Add(card);
    }
    
    // Remove a card from the top of the deck and add it to the hand
    public void DrawCard(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            if (cards.Count > 0)
            {
                Card card = cards[0];
                cards.RemoveAt(0);
                hand.AddCard(card);
            }
        }
    }
}

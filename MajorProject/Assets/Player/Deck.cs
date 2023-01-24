using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;
using UnityEngine.EventSystems;

public class Deck : MonoBehaviour, IPointerDownHandler
{
    public List<GameObject> cards = new List<GameObject>();
    
    [Header("References")]
    public GameObject hand;
    public List<GameObject> cardPrefabs;

    public void Init(int i)
    {
        Debug.Log(i);
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("OnPointerDown");
        DrawCard(1);
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
            // Instantiate the card with the correct ID
            GameObject newCard = Instantiate(cardPrefabs[cardId], transform, false) as GameObject;
            newCard.transform.Translate(0, 0, 1);
            // Put it on the player canvas
            newCard.GetComponent<Interactable>().updateCanvas(GetComponentInParent<Canvas>());
            AddCard(newCard);
        }
    }

    // Add a card to the bottom of the deck
    public void AddCard(GameObject card)
    {
        cards.Add(card);
        card.transform.SetParent(transform);
    }
    
    // Remove a card from the top of the deck and add it to the hand
    public void DrawCard(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            if (cards.Count > 0)
            {
                GameObject topCard = cards[0];
                // Check if hand is full?  This implementation "burns" cards if the hand is full
                cards.RemoveAt(0);
                hand.GetComponent<Hand>().AddCard(topCard);
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;

public class Deck : MonoBehaviour, IPointerDownHandler
{
    public List<GameObject> cards;

    [Header("References")]
    public GameObject player;
    public GameObject hand;
    public List<GameObject> cardPrefabs;
    
    #region Card Functions

    // Add a card to the bottom of the deck
    public void AddCard(GameObject card)
    {
        cards.Add(card);
        card.transform.SetParent(transform);
        card.GetComponent<Interactable>().homePosition = Vector3.zero;
        card.GetComponent<Interactable>().ReturnHome();
        // Shift the card to z=1 so it is behind the deck
        // card.transform.position = new Vector3(card.transform.position.x, card.transform.position.y, -1);
    }
    
    // Remove a card from the top of the deck and add it to the hand
    public void DrawCard(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            if (cards.Count > 0)
            {
                GameObject topCard = cards[0];
                // Maybe Check if hand is full?  This implementation "burns" cards if the hand is full
                cards.RemoveAt(0);
                hand.GetComponent<Hand>().AddCard(topCard);
            }
        }
    }

    #endregion
    
    #region Callback Handlers

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

    #endregion
    
    public void GenerateDeck(List<int> cardIds)
    {
        foreach (int cardId in cardIds)
        {
            // Instantiate the card with the correct ID
            //GameObject newCard = PhotonNetwork.Instantiate(cardPrefabs[cardId].name, transform.position, Quaternion.identity);
            GameObject newCard = InstantiateNewCard(cardPrefabs[cardId]);
            AddCard(newCard);
        }
    }

    // Helper function to instantiate a card and return it
    public GameObject InstantiateNewCard(GameObject card)
    {
        GameObject newCard = Instantiate(card, transform.position, Quaternion.identity);
        // Rename the card to not include (Clone)
        newCard.name = card.name;
        // Shift the card to z=1 so it is behind the deck
        newCard.transform.Translate(0, 0, 1);
        // Set the owner
        newCard.GetComponent<Card>().owner = player;
        // Put it on the player canvas
        newCard.GetComponent<Interactable>().updateCanvas(GetComponentInParent<Canvas>());
        return newCard;
    }
}

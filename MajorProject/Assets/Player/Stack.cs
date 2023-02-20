using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class Stack : MonoBehaviour, IPunObservable
{
    public List<GameObject> cards;
    
    public List<Delegate> cardEffects;
    
    [Header("References")]
    //public GameObject owner;      // Reference to the player GameObject that the stack belongs to (not used)
    public Player owner;            // Reference to the player script that the stack belongs to
    public GameObject lifeText;     // Reference to the player's life text
    public TextMeshProUGUI nameText;    // Reference to the player's name text
    public Transform canvasTransform;   // Reference to this stack's Canvas

    #region Photon
    
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
        //     owner = PhotonView.Find(ownerID).gameObject.GetComponent<Player>();
        // }
    }

    #endregion
    
    #region Card Functions

    public void AddCard(GameObject card)
    {
        Debug.Log("Adding card " + card.name + " to stack");
        // card.transform.SetParent(transform);
        // Set the home position to the stack (I have no idea why y=40)
        Vector3 newPosition = new Vector3(0, 40, 0);
        card.GetComponent<Interactable>().homePosition = newPosition;
        card.GetComponent<Interactable>().ReturnHome();
        cards.Add(card);
    }

    #endregion

    #region Card Resolving Things

    // Variables to keep track of card effects
    private bool counterNextCard = false;
    private bool counterAllCards = false;
    private bool stabilizeLife = false;
    private bool lifeDoubler = false;
    private bool damageDoubler = false;
    private bool goToHand = false;
    private bool preventCardDraw = false;
    private bool lostLifeThisRound = false;
    private bool gainedLifeThisRound = false;
    private bool drawnCardsThisRound = false;

    // Resolve the effects of all cards on the stack, most recent first, and return them to their original owners
    public void ResolveEffects()
    {
        Debug.Log("Resolving effects");
        // Error checking
        if (cardEffects.Count == 0)
        {
            Debug.LogError("No effects to resolve");
            return;
        }
        if (cardEffects.Count != cards.Count)
        {
            Debug.LogError("Error: Number of effects does not match number of cards");
            return;
        }
        
        // Loop through the cards in reverse order
        for (int i = cards.Count - 1; i >= 0; i--)
        {
            if (!counterNextCard)
            {
                // Trigger the effect (cardEffects have the same indexes as cards)
                cardEffects[i].DynamicInvoke();
            }
            else
            {
                Debug.Log("Card Countered");
                if (!counterAllCards)
                {
                    counterNextCard = false;
                }
            }
            
            // Card has been resolved, Destroy it and instantiate a new one in the deck
            GameObject originalOwner = cards[i].GetComponent<Card>().owner;
            
            // Only return a card to a deck if this is the client with access to that deck (or hand)
            if (originalOwner.GetComponent<Player>().myDeck != null)
            {
                Deck originalDeck = originalOwner.GetComponent<Player>().myDeck.GetComponent<Deck>();
                GameObject newCard = originalDeck.InstantiateNewCard(cards[i]);
                
                if (goToHand) // Some cards will go to a hand instead of returning to a deck after played
                {
                    goToHand = false;
                    owner.myHand.GetComponent<Hand>().AddCard(newCard);
                }
                else
                {
                    // Call AddCard on the owner's deck
                    originalDeck.AddCard(newCard);
                }
            }
            // Destroy the card
            //PhotonNetwork.Destroy(cards[i]);
            
            // Photon RPC to the original owner to destroy the card
            GameManager.NetworkManager.PV.RPC("DestroyCard", 
                originalOwner.GetComponent<PhotonView>().Owner, 
                cards[i].GetComponent<PhotonView>().ViewID);
        }
        // Photon RPC to update the life text
        GameManager.NetworkManager.PV.RPC("SetPlayerLife", RpcTarget.All, 
            owner.gameObject.GetComponent<PhotonView>().ViewID, owner.life);

        // Clear the stack on all clients
        GameManager.NetworkManager.PV.RPC("ClearStack", RpcTarget.All,
            GetComponent<PhotonView>().ViewID);
    }

    // Reset all card effect variables to their default values
    private void ResetVariables()
    {
        counterNextCard = false;
    }

    #endregion

    #region Card Effects

    // Card ID: 0
    public void Tony()
    {
        Debug.Log("Tony effect");
        LoseLife(4);
    }
    
    // Card ID: 1
    public void George()
    {
        Debug.Log("George effect");
        GainLife(4);
    }
    
    // Card ID: 2
    public void Nope()
    {
        Debug.Log("Counter next card");
        counterNextCard = true;
    }
    
    // Card ID: 3
    public void Hibernate()
    {
        Debug.Log("Hibernate effect");
        lifeDoubler = true;
    }
    
    // Card ID: 4
    public void BearMarket()
    {
        Debug.Log("Bear Market effect");
        damageDoubler = true;
    }
    
    // Card ID: 5
    public void Stabilize()
    {
        Debug.Log("Stabilize effect");
        stabilizeLife = true;
    }
    
    // Card ID: 6
    public void Scavenge()
    {
        Debug.Log("Stabilize effect");
        DrawCards(2);
    }
    
    // Card ID: 7
    public void EatSand()
    {
        LoseLife(6);
        DrawCards(3);
    }
    
    // Card ID: 8
    public void BadBerries()
    {
        LoseLife(5);
        goToHand = true;
    }
    
    // Card ID: 9
    public void GoodBerries()
    {
        GainLife(1);
        goToHand = true;
    }
    
    // Card ID: 10
    public void LockDown()
    {
        preventCardDraw = true;
    }

    // Card ID: 11
    public void Wither()
    {
        DrawCards(2);
        int cardsInHand = owner.GetComponent<Hand>().cards.Count;
        LoseLife(cardsInHand);
    }
    
    // Card ID: 12
    public void Cull()
    {
        // If you haven't gained life this round, lose 5 life
        if (!gainedLifeThisRound)
        {
            LoseLife(5);
        }
    }
    
    // Card ID: 13
    public void Infestation()
    {
        // Lose 2 life for each card on the stack
        LoseLife(cards.Count * 2);
    }
    
    // Card ID: 14
    public void Hoard()
    {
        // Gain 2 life for each card on the stack
        GainLife(cards.Count * 2);
    }
    
    // Card ID: 15
    public void FallenTree()
    {
        // If you've drawn cards this round, lose 7 life
        if (drawnCardsThisRound)
        {
            LoseLife(7);
        }
    }
    
    // Card ID: 16
    public void BigHands()
    {
        // Gain 1 life for each card in your hand
        GainLife(owner.GetComponent<Hand>().cards.Count + 1);
    }
    
    // Card ID: 17
    public void SwiftEnd()
    {
        // If you have the end card, increase life lose this round by 3
        //TODO: NEED END CARD FUNCTIONALITY
    }
    
    // Card ID: 24
    public void SurpriseTrain()
    {
        counterAllCards = true;
    }
    
    #endregion

    #region Card Effect Helper Functions
    
    private void GainLife(int amount)
    {
        if (stabilizeLife)
        {
            stabilizeLife = false;
        }
        else
        {
            if (lifeDoubler)
            {
                amount *= 2;
                lifeDoubler = false;
            }
            owner.life += amount;
            gainedLifeThisRound = true;
        }
    }
    
    private void LoseLife(int amount)
    {
        if (stabilizeLife)
        {
            stabilizeLife = false;
        }
        else
        {
            if (damageDoubler)
            {
                amount *= 2;
                damageDoubler = false;
            }
            owner.life -= amount;
            lostLifeThisRound = true;
        }
    }
    
    private void DrawCards(int amount)
    {
        if (preventCardDraw)
        {
            preventCardDraw = false;
        }
        else
        {
            owner.myDeck.GetComponent<Deck>().DrawCard(amount);
        }
    }

    #endregion
    
    public Transform GetCanvasTransform()
    {
        return canvasTransform;
    }

    #region Callback Handlers

    // Start is called before the first frame update
    void Start()
    {
        cardEffects = new List<Delegate>();
        nameText.text = owner.playerName;
    }

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        lifeText.GetComponent<TextMeshProUGUI>().text = owner.life.ToString();
    }

    #endregion
}

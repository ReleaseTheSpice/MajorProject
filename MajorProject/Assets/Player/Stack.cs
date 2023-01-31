using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stack : MonoBehaviour
{
    public List<GameObject> cards;
    
    public List<Delegate> cardEffects;
    
    [Header("References")]
    //public GameObject owner;    // Reference to the player GameObject that the stack belongs to (not used)
    public Player owner;       // Reference to the player script that the stack belongs to
    public Transform canvasTransform;   // Reference to this stack's Canvas

    #region Card Functions

    public void AddCard(GameObject card)
    {
        Debug.Log("Adding card " + card.name + " to stack");
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
    
    // Variables to keep track of life
    private int lifeChange = 0;
    
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
        
        for (int i = cards.Count - 1; i >= 0; i--)
        {
            if (!counterNextCard)
            {
                // Trigger the effect
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
            if (goToHand) // Some cards will go to a hand instead of returning to a deck after played
            {
                goToHand = false;
                owner.myHand.GetComponent<Hand>().AddCard(cards[i]);
            }
            else
            {
                // Return the card to its original owner
                GameObject originalOwner = cards[i].GetComponent<Card>().owner;
                // Call AddCard on the owner's deck
                originalOwner.GetComponentInChildren<Deck>().AddCard(cards[i]);
            }
        }
        
        // Clear the stack
        cards.Clear();
        cardEffects.Clear();
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
    
    // public void NopeAll()
    // {
    //     Debug.Log("Counter all cards");
    //     counterNextCard = true;
    //     counterAllCards = true;
    // }

    // Card ID: 12
    public void Wither()
    {
        DrawCards(2);
        int cardsInHand = owner.GetComponent<Hand>().cards.Count;
        LoseLife(cardsInHand);
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
            lifeChange += amount;
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
            lifeChange -= amount;
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
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #endregion
}

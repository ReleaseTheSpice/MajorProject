using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Photon.Pun;

public class Player : MonoBehaviour
{
    #region Public Fields

    public GameObject myDeck;
    public GameObject myHand;
    
    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalPlayerInstance;
    
    public int life;
    public string playerName;
    
    #endregion

    #region Callback Handlers

    // Start is called before the first frame update
    void Start()
    {
        life = 20;
        
        List<int> deckCode = new List<int>(){ 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2 };
        myDeck.GetComponent<Deck>().GenerateDeck(deckCode);
    }

    private void Awake()
    {
        // #Important
        // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
        if (PhotonView.Get(this).IsMine)
        {
            Player.LocalPlayerInstance = this.gameObject;
        }
        // #Critical
        // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
        DontDestroyOnLoad(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        // Need to have "using Photon.Pun;" at the top of the file
        // if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
        // {
        //     return;
        // }
        if (life <= 0)
        {
            Debug.Log("You lose!");
            GameManager.Instance.LeaveRoom();
        }

        //lifeText.GetComponent<TextMeshProUGUI>().text = life.ToString();
    }
    
    #endregion

    #region Public Methods



    #endregion
}

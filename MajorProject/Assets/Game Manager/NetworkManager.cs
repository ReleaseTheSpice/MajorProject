using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    #region Public Fields

    public static NetworkManager Instance;
    public PhotonView PV;

    #endregion
    
    #region MonoBehaviour Callbacks

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        PV = PhotonView.Get(this);
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
    
    #region Public Methods
    
    [PunRPC]
    public void SetPlayerName(int viewID, string name)
    {
        PhotonView.Find(viewID).GetComponent<Player>().playerName = name;
    }
    
    [PunRPC]
    public void SetStackOwner(int viewID, int ownerID)
    {
        PhotonView.Find(viewID).GetComponent<Stack>().owner = PhotonView.Find(ownerID).GetComponent<Player>();
    }
    
    [PunRPC]
    public void SetPlayerLife(int viewID, int life)
    {
        PhotonView.Find(viewID).GetComponent<Player>().life = life;
    }
    
    #endregion
}

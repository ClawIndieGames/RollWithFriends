﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class LobbyController : MonoBehaviourPunCallbacks
{
    #region Fields and properties
    [SerializeField] RectTransform playerListContent;

    #endregion

    #region Public methods
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        UpdatePlayerList();

    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        UpdatePlayerList();
    }
    
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        UpdatePlayerList();
    }

    #endregion


    #region Private methods	

    void Start()
    {

    }

    void Update()
    {

    }


    private void UpdatePlayerList()
    {
        if (PhotonNetwork.CurrentRoom.Players.Any())
        {
            var previousElementPosition = Vector2.zero;
            foreach (var p in PhotonNetwork.CurrentRoom.Players)
            {
                var obj = PhotonNetwork.Instantiate(
                           Constants.PlayerListItemPrefabName,
                           playerListContent.transform.position,
                           Quaternion.identity);

                obj.transform.SetParent(playerListContent.transform);

                previousElementPosition =
                    obj.GetComponent<RectTransform>().anchoredPosition
                    = Vector2.zero + previousElementPosition;
                previousElementPosition += new Vector2(0, -50); // 50 is the item height

                var nameToShow = PhotonNetwork.LocalPlayer.NickName == p.Value.NickName
                    ? p.Value.NickName + " (you)"
                    : p.Value.NickName;

                obj.GetComponentInChildren<TextMeshProUGUI>().text = nameToShow;
            }
        }
    }

    #endregion
}

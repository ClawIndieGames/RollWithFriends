﻿using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkConnectionManager : MonoBehaviourPunCallbacks
{
    #region Fields and properties
    [SerializeField] Button joinRoomButton;

    [SerializeField] TMP_InputField roomNameInputField;

    [SerializeField] GameObject lobbyWindow;

    [SerializeField] TextMeshProUGUI roomNameTextMesh;
    [SerializeField] TextMeshProUGUI connectingLabelTextMesh;

    [SerializeField] LobbySettingsController lobbyController;

    #endregion

    #region Public methods
    public void LoadMultiplayerLevel()
    {
        photonView.RPC(nameof(LoadGameRPC), RpcTarget.All);
    }

    public void ConnectToMaster()
    {
        PhotonNetwork.OfflineMode = false;

        if (!PlayerPrefs.HasKey(Constants.PlayerPrefKeyUser))
        {
            Debug.LogError("No player found!");
            return;
        }

        PhotonNetwork.NickName = PlayerPrefs.GetString(Constants.PlayerPrefKeyUser);
        PhotonNetwork.GameVersion = Constants.GameVersion;

        PhotonNetwork.ConnectUsingSettings();

    }

    public void JoinOrCreateRoom()
    {
        if (!PhotonNetwork.IsConnected)
        {
            return;
        }
        var roomName = roomNameInputField.text != string.Empty
             ? roomNameInputField.text
             : Guid.NewGuid().ToString();

        var roomOptions = new RoomOptions();
        PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
        //PhotonNetwork.JoinRandomRoom();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        Debug.Log(cause);
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();

        joinRoomButton.interactable = true;
        connectingLabelTextMesh.text = "Connected";
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        joinRoomButton.interactable = false;
        lobbyWindow.SetActive(true);
        SetRoomDetailsData();
    }

    /// <summary>
    /// When ANOTHER client enters the room
    /// </summary>    
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        Debug.Log(PhotonNetwork.CurrentRoom.Players);
    }


    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);

    }
    #endregion


    #region Private methods	

    void Start()
    {
        joinRoomButton.interactable = false;
        connectingLabelTextMesh.gameObject.SetActive(true);

        PhotonNetwork.AutomaticallySyncScene = true;
        if (!PhotonNetwork.IsConnected)
        {
            ConnectToMaster();
        }
        else
        {            
            connectingLabelTextMesh.text = "Connected";            
            SetRoomDetailsData();
            lobbyWindow.SetActive(true);
        }
    }

    void Update()
    {

    }

    private void SetRoomDetailsData()
    {
        roomNameTextMesh.text = PhotonNetwork.CurrentRoom.Name;
    }

    IEnumerator LoadLevelAsyncRoutine(string levelName)
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        yield return null;

        AsyncOperation levelAsync = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Additive);

        levelAsync.allowSceneActivation = false;

        while (!levelAsync.isDone)
        {
            if (levelAsync.progress >= 0.9f)
            {
                levelAsync.allowSceneActivation = true;

            }

            yield return null;
        }

        AsyncOperation gameAsync = SceneManager.LoadSceneAsync(Constants.SceneNameGame, LoadSceneMode.Additive);
        gameAsync.allowSceneActivation = false;

        while (!gameAsync.isDone)
        {
            if (gameAsync.progress >= 0.9f)
            {
                gameAsync.allowSceneActivation = true;
            }
            yield return null;
        }

        var gameScene = SceneManager.GetSceneByName(Constants.SceneNameGame);
        SceneManager.SetActiveScene(gameScene);
        UpdateLobbyManagerAndInitLevel(levelName);
        SceneManager.UnloadSceneAsync(Constants.SceneNameMultiplayerLobby);

        yield return null;
    }

    /// <summary>
    /// Updates the lobby manager with the desired settings
    /// and initializes the level manager so.
    /// </summary>
    void UpdateLobbyManagerAndInitLevel(string levelName)
    {
        var roudTimeSeconds = lobbyController.roundTimeValueMinutes != 0
             // value in minutes * 60 to make it seconds
             ? lobbyController.roundTimeValueMinutes * 60f
             : Constants.DefaultRoundTimeSeconds;

        var roomSettings = new MultiplayerRoomSettings(
            roomPlayerCount: PhotonNetwork.CurrentRoom.PlayerCount,
            roomName: PhotonNetwork.CurrentRoom.Name,
            roomMasterClientId: PhotonNetwork.CurrentRoom.masterClientId,
            levelName: levelName,
            levelCodeName: levelName,// TODO JS: level name is not its codename
            roundTimeSeconds: roudTimeSeconds);

        LobbyManager.instance.SetMultiplayerRoomDetails(roomSettings);

        var levelManager = FindObjectOfType<LevelManager>();

        if (levelManager != null)
        {
            levelManager.InitializeMultiplayer();

        }
    }

    [PunRPC]
    void LoadGameRPC()
    {
        var levelNameToLoad = Constants
            .MultiplayerLevelsArray[lobbyController.levelSelectionDropdown.value];


        StartCoroutine(LoadLevelAsyncRoutine(levelNameToLoad));
    }
    #endregion
}

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class PlayerController : MonoBehaviour
{
    #region Fields and properties

    [SerializeField] Rigidbody rb;

    private Checkpoint lastCheckpointReached;

    public static event Action<PlayerController> OnPlayerReachedEnd = delegate { };

    public static event Action OnPlayerResetLevel = delegate { };

    #endregion

    #region Public methods

    #endregion


    #region Private methods	

    void Start()
    {

    }

    void Update()
    {
        if (Input.GetButtonDown(Constants.ButtonResetLevel))
        {
            OnPlayerResetLevel();
            RestartAtStart();
        }

        if (Input.GetButtonDown(Constants.ButtonResetCheckpoint))
        {
            RespawnAtLastCheckpoint();
        }

    }

    void RestartAtStart()
    {
        Freeze(false);

        transform.position =
            GameObject.FindObjectsOfType<Checkpoint>()
                .Where(c => c.checkpointType == Checkpoint.CheckpointType.Start)
                .FirstOrDefault()
                .respawnPoint.transform.position;
    }

    void RespawnAtLastCheckpoint()
    {
        Freeze(false);

        if (lastCheckpointReached != null
            && lastCheckpointReached.respawnPoint != null)
        {
            transform.position =
                lastCheckpointReached.respawnPoint.transform.position;
        }
        else
        {
            RestartAtStart();
        }
    }

    private void Freeze(bool isKinematic)
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = isKinematic;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Constants.TagCheckpoint))
        {
            var cp = other.transform.GetComponent<Checkpoint>();
            lastCheckpointReached = cp;

            if (cp.checkpointType == Checkpoint.CheckpointType.End)
            {
                Freeze(true);
                OnPlayerReachedEnd(this);
            }
        }

        if (other.CompareTag(Constants.TagDeath))
        {
            RespawnAtLastCheckpoint();
        }
    }

    #endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{

    PlayerLocomotion playerLocomotion;

    // Start is called before the first frame update
    void Awake()
    {
        playerLocomotion = GetComponent<PlayerLocomotion>();
    }

    private void FixedUpdate()
    {
        playerLocomotion.HandleMovement();
    }
}

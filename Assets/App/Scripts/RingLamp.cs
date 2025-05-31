using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingLamp : MonoBehaviour
{
    [SerializeField] protected GameObject lampDeactive = null;
    [SerializeField] protected GameObject lampActive = null;

    private bool stateActive = false;

    public bool StateActive
    {
        get
        {
            return stateActive;
        }
        set
        {
            stateActive = value;
            lampDeactive.SetActive(!stateActive);
            lampActive.SetActive(stateActive);
        }
    }
}

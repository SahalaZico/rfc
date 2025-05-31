using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FSMAction
{
    [SerializeField] protected string name = "Default";
    public string Name { get { return name; } }

    [SerializeField] protected string animation = "idle";
    public string Animation
    {
        get
        {
            return animation;
        }
    }

    [SerializeField] protected float weight = 0f;
    public float Weight
    {
        get
        {
            return weight;
        }
    }

    [SerializeField] protected float distance = 1.4f;
    public float Distance
    {
        get
        {
            return distance;
        }
    }

    [SerializeField] protected float maxCooldown = 3f;
    public float MaxCooldown
    {
        get
        {
            return maxCooldown;
        }
    }

    protected float cooldown = 0f;
    public float Cooldown
    {
        get
        {
            return cooldown;
        }
        set
        {
            cooldown = value;
            if (cooldown < 0f)
                cooldown = 0f;
        }
    }

    public FSMAction()
    {
        name = "Default";
        animation = "idle";
        weight = 0f;
        distance = 1.4f;
        maxCooldown = 3f;
        cooldown = 0f;
    }
}

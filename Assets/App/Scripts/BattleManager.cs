using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BattleManager : MonoBehaviour
{
    public enum State
    {
        Pause = 0,
        Play = 1,
        Wait = 2,
        Finish = 3
    }

    public static BattleManager Instance { get; private set; }

    [SerializeField] protected float timerGoal = 10f;
    [SerializeField] protected int targetGoal = 20;
    public int TargetGoal
    {
        get
        {
            return targetGoal;
        }
        set
        {
            targetGoal = value;
        }
    }

    [SerializeField] protected FSMMotor avatarGoal = null;
    public FSMMotor AvatarGoal
    {
        get
        {
            return avatarGoal;
        }
        set
        {
            avatarGoal = value;
        }
    }

    [SerializeField] protected Collider2D ringCollider = null;
    [SerializeField] protected List<Transform> goals = new List<Transform>();
    public Transform TargetTransGoal
    {
        get
        {
            return goals[targetGoal].GetChild(0);
        }
    }

    public SpriteRenderer TargetSpriteGoal
    {
        get
        {
            return goals[targetGoal].GetChild(1).GetComponent<SpriteRenderer>();
        }
    }

    [SerializeField] protected List<RingLamp> lamps = new List<RingLamp>();
    [SerializeField] protected List<FSMMotor> actors = new List<FSMMotor>();
    public List<FSMMotor> Actors
    {
        get
        {
            return actors;
        }
    }

    [SerializeField] protected State currentState = State.Pause;
    protected Sequence stageSequence = null;
    public State CurrentState
    {
        get
        {
            return currentState;
        }
        private set
        {
            currentState = value;
            switch (currentState) {
                case State.Play:
                    if (stageSequence != null)
                    {
                        if (stageSequence.active)
                            stageSequence.Kill();
                    }
                    stageSequence = DOTween.Sequence();
                    stageSequence.Append(DOVirtual.Float(timerGoal, 0f, timerGoal, (floatUpdate) => { }));
                    stageSequence.AppendCallback(() => {
                        //UIGameplay.Instance.StopRouletteWheel();
                        TakeGoal = true;
                    });
                    DOVirtual.Float(timerGoal - 2.5f, 0f, timerGoal - 2.5f, (floatUpdate) => { }).OnComplete(() => {
                        UIGameplay.Instance.StopRouletteWheel();
                    });
                    break;
                default:
                    break;
            }
        }
    }

    [SerializeField] protected bool takeGoal = false;
    public bool TakeGoal
    {
        get
        {
            return takeGoal;
        }
        set
        {
            takeGoal = value;
            switch (takeGoal) {
                case true:
                    avatarGoal = null;
                    float minDistance = float.MaxValue;
                    for (int i = 0; i < actors.Count; i++)
                    {
                        if (actors[i] == null)
                            continue;

                        float currDistance = Vector2.Distance(actors[i].transform.position, goals[targetGoal].GetChild(0).position);
                        if (currDistance < minDistance)
                        {
                            //avatarGoal = actors[i];
                            minDistance = currDistance;
                        }
                    }
                    //if (avatarGoal != null)
                    //{
                    //    avatarGoal.SetToVictim(true);
                    //    ringCollider.isTrigger = true;
                    //}
                    ringCollider.isTrigger = true;
                    break;
                default:
                    avatarGoal = null;
                    ringCollider.isTrigger = false;
                    break;
            }
        }
    }

    [SerializeField] protected bool pause = false;
    public bool Pause
    {
        get
        {
            return pause;
        }
        set
        {
            pause = value;

            for (int i = 0; i < actors.Count; i++)
            {
                if (actors[i] == null)
                    continue;

                actors[i].Pause = pause;
            }
        }
    }

    public void ChangeTargetGoal()
    {
        if (avatarGoal != null)
            avatarGoal.SetToVictim(false);

        avatarGoal = null;
        float minDistance = float.MaxValue;
        for (int i = 0; i < actors.Count; i++)
        {
            if (actors[i] == null)
                continue;

            if (actors[i].OnHit)
                continue;

            if (actors[i].IsDead())
                continue;

            float currDistance = Vector2.Distance(actors[i].transform.position, goals[targetGoal].GetChild(0).position);
            if (currDistance < minDistance)
            {
                //avatarGoal = actors[i];
                minDistance = currDistance;
            }
        }
    }

    public FSMMotor GetAvailableTarget(FSMMotor request)
    {
        List<FSMMotor> availableTargets = new List<FSMMotor>();
        for (int i = 0; i < actors.Count; i++)
        {
            if (actors[i] == null)
                continue;

            if (actors[i].IdView == request.IdView)
                continue;

            if (actors[i].OnHit)
                continue;

            availableTargets.Add(actors[i]);
        }

        if (availableTargets.Count <= 0)
            return null;

        return availableTargets[Random.Range(0, availableTargets.Count)];
    }

    public void ResetBattle()
    {
        Pause = true;
        TakeGoal = false;
        for (int i = 0; i < actors.Count; i++)
        {
            if (actors[i] == null)
                continue;

            actors[i].ResetMotor();
        }

        for (int i = 0; i < lamps.Count; i++)
        {
            if (lamps[i] == null)
                continue;

            lamps[i].StateActive = false;
        }
    }

    public void Play()
    {
        Pause = false;
        TakeGoal = false;
        CurrentState = State.Play;
    }

    public void Stop()
    {
        Pause = true;
        CurrentState = State.Finish;
        UIGameplay.Instance.PlayRingBell();
        UIGameplay.Instance.OnShowResult();

        for (int i = 0; i < lamps.Count; i++)
        {
            if (lamps[i] == null)
                continue;

            lamps[i].StateActive = true;
        }
        //UIGameplay.Instance.OnFinishReached();
    }

    [SerializeField] private float range = 10.0f;

    public Vector3 GetRandomPointOnNavMesh(Vector3 center)
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPos = center + Random.insideUnitSphere * range;
            if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }
        return center;
    }

    [SerializeField] private float sampleMaxDistance = 2f;
    [SerializeField] private float checkRadius = 0.5f;
    [SerializeField] private LayerMask obstacleMask;

    public Vector3 GetSafeRandomNavMeshPoint(Vector3 center)
    {
        for (int i = 0; i < 50; i++)
        {
            Vector3 randomPos = center + Random.insideUnitSphere * range;
            randomPos.y = center.y;

            if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, sampleMaxDistance, NavMesh.AllAreas))
            {
                // Check if there's an obstacle (like a wall) at that point
                if (!Physics.CheckSphere(hit.position, checkRadius, obstacleMask))
                {
                    return hit.position;
                }
            }
        }

        return center;
    }

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }
}

using DG.Tweening;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FSMMotor : MonoBehaviour
{
    public enum State
    {
        Idle = 0,
        Chase = 1,
        Action = 2,
        Hit = 3,
        Dead = 4,
    }

    [SerializeField] protected int idView = -1;
    public int IdView
    {
        get
        {
            return idView;
        }
    }

    [SerializeField] protected int startHit = 0;
    [SerializeField] protected Rigidbody2D body2D = null;
    [SerializeField] protected Animation anim = null;
    [SerializeField] protected SkeletonAnimation skeleRoost = null;
    public SkeletonAnimation SkeleRoost
    {
        get
        {
            return skeleRoost;
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
            switch (pause)
            {
                case true:
                    if (state != State.Dead)
                        body2D.linearVelocity = Vector2.zero;
                    if (state == State.Dead)
                        return;
                    SwitchStateTo(State.Idle);
                    break;
                default:
                    break;
            }
        }
    }

    [SerializeField] protected State state = State.Idle;
    [SerializeField] protected float maxHp = 100f;

    [SerializeField] protected float distanceTarget = 5f;
    [SerializeField] protected float distanceAction = 0.25f;

    [SerializeField] protected float speed = 0.25f;
    [SerializeField] protected float angularSpeed = 100f;

    [SerializeField] protected List<FSMAction> availableActions = new List<FSMAction>();
    public List<FSMAction> AvailableActions
    {
        get
        {
            return availableActions;
        }
    }

    [SerializeField] protected ParticleSystem vfxHit = null;
    [SerializeField] protected FSMAnimationListener animationListener = null;

    protected FSMMotor currentTarget = null;
    protected FSMAction cacheAction = null;
    protected float currentHp = 0f;

    protected bool onAction = false;
    public bool OnAction
    {
        get
        {
            return onAction;
        }
        set
        {
            onAction = value;
            if (!onAction)
            {
                cacheAction = null;
                if (state == State.Dead)
                    return;

                SwitchStateTo(State.Idle);
            }
        }
    }

    protected bool onHit = false;
    public bool OnHit
    {
        get
        {
            return onHit;
        }
    }
    protected bool onDodge = false;

    protected Sequence customAct = null;
    protected Vector3 positionAwake = Vector3.zero;
    protected Vector3 eulerAwake = Vector3.zero;
    protected float currentTick = 0f;
    protected int currentHit = 0;

    public bool IsDead()
    {
        return state == State.Dead;
    }

    protected void UpdateActions(float delta)
    {
        for (int i = 0; i < availableActions.Count; i++)
        {
            if (availableActions[i] == null)
                continue;

            if (availableActions[i].Cooldown <= 0f)
                continue;

            availableActions[i].Cooldown -= delta;
        }
    }

    protected FSMAction FetchAction(float delta)
    {
        List<FSMAction> action = new List<FSMAction>();

        for (int i = 0; i < availableActions.Count; i++)
        {
            if (availableActions[i] == null)
                continue;

            if (availableActions[i].Cooldown > 0f)
                continue;

            action.Add(availableActions[i]);
        }

        return action[Random.Range(0, action.Count)];
    }

    public void SetToVictim(bool state)
    {
        gameObject.layer = LayerMask.NameToLayer(state == true ? "Victim" : "ActorBody");
    }

    protected void UpdateHitState()
    {
        if (OnAction)
            onAction = false;
    }

    protected void UpdateActionState()
    {
        if (OnAction && cacheAction != null) {
            if (cacheAction.Animation == "charge")
            {
                Vector2 enemyDirection = currentTarget.transform.position - transform.position;
                enemyDirection = enemyDirection.normalized;
                Quaternion targetRotation = Quaternion.LookRotation(transform.forward, enemyDirection);
                Quaternion rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, angularSpeed * Time.deltaTime);

                body2D.SetRotation(rotation);
            }
            return;
        }

        FSMAction action = FetchAction(Time.deltaTime);

        if (action == null)
        {
            body2D.linearVelocity = Vector2.zero;
            SwitchStateTo(State.Chase);
            return;
        }

        OnAction = true;
        cacheAction = action;
        if (action.Animation == "charge")
        {
            //play tween
            if (customAct != null)
            {
                if (customAct.active)
                    customAct.Kill();
            }
            customAct = DOTween.Sequence();
            customAct.AppendCallback(() =>
            {
                Vector3 chargeMovement = transform.position + (transform.right * 0.5f);
                body2D.DOMove(chargeMovement, 0.6f);
            });
            customAct.AppendInterval(0.6f);
            customAct.AppendCallback(() =>
            {
                Vector3 chargeMovement = transform.position + (transform.right * 0.5f);
                body2D.DOMove(chargeMovement, 0.6f);
            });
            customAct.AppendInterval(0.6f);
            customAct.AppendCallback(() =>
            {
                if (state == State.Dead)
                    return;
                OnAction = false;
                SwitchStateTo(State.Idle);
            });
            return;
        }
        anim.Play(action.Animation);

        if (action.Animation == "dodge")
        {
            onDodge = true;
            if (customAct != null)
            {
                if (customAct.active)
                    customAct.Kill();
            }
            Vector3 dodgePosition = BattleManager.Instance.GetSafeRandomNavMeshPoint(transform.position);
            customAct = DOTween.Sequence();
            customAct.Append(body2D.DOMove(dodgePosition, 0.45f)); //replace with custom jump point
            customAct.AppendCallback(() =>
            {
                if (state == State.Dead)
                    return;
                OnAction = false;
                onDodge = false;
                SwitchStateTo(State.Idle);
            });
            Debug.Log("It is dodge time");
        }
    }

    protected void UpdateChaseState()
    {
        if (currentTarget == null)
        {
            body2D.linearVelocity = Vector2.zero;
            SwitchStateTo(State.Idle);
            return;
        }

        float distance = Vector2.Distance(currentTarget.transform.position, transform.position);
        if (distance <= distanceAction)
        {
            body2D.linearVelocity = Vector2.zero;
            SwitchStateTo(State.Action);
            currentTick = 0f;
            return;
        }

        Vector2 enemyDirection = currentTarget.transform.position - transform.position;
        enemyDirection = enemyDirection.normalized;

        Quaternion targetRotation = Quaternion.LookRotation(transform.forward, enemyDirection);
        Quaternion rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, angularSpeed * Time.deltaTime);

        body2D.SetRotation(rotation);
        body2D.linearVelocity = transform.up * speed;

        currentTick += Time.deltaTime;
        if (currentTick >= 5f)
        {
            body2D.linearVelocity = Vector2.zero;
            SwitchStateTo(State.Idle);
            currentTick = 0f;
        }
    }

    protected void UpdateIdleState()
    {
        currentTarget = BattleManager.Instance.GetAvailableTarget(this);
        if (currentTarget == null)
            return;

        float distance = Vector2.Distance(currentTarget.transform.position, transform.position);
        if (distance <= distanceTarget)
        {
            SwitchStateTo(State.Chase);
        }
    }

    protected void SwitchStateTo(State newState)
    {
        state = newState;
        switch (state)
        {
            case State.Idle:
                skeleRoost.AnimationState.SetAnimation(0, "idle", true);
                break;
            case State.Chase:
                skeleRoost.AnimationState.SetAnimation(0, "walk", true);
                break;
            case State.Action:
                break;
            case State.Hit:
                break;
            case State.Dead:
                break;
            default:
                break;
        }
    }

    protected void FSMUpdate()
    {
        switch (state)
        {
            case State.Idle:
                UpdateIdleState();
                break;
            case State.Chase:
                UpdateChaseState();
                break;
            case State.Action:
                UpdateActionState();
                break;
            case State.Hit:
                UpdateHitState();
                break;
            case State.Dead:
                break;
            default:
                break;
        }

        UpdateActions(Time.deltaTime);
    }

    void Awake()
    {
        positionAwake = transform.position;
        eulerAwake = transform.eulerAngles;
    }

    // Start is called before the first frame update
    void Start()
    {
        currentHp = maxHp;
        currentHit = startHit;

        SwitchStateTo(State.Idle);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (pause)
        {
            if (BattleManager.Instance.CurrentState == BattleManager.State.Finish && BattleManager.Instance.AvatarGoal != null)
            {
                if (BattleManager.Instance.AvatarGoal.IdView != idView)
                    body2D.linearVelocity = Vector2.zero;
            }
            return;
        }

        if (state == State.Dead)
            return;

        if (onHit)
            return;

        if (onDodge)
            return;

        FSMUpdate();
    }

    private IEnumerator IEnumRestoreAlmostFall()
    {
        yield return new WaitForSeconds(2f);

        if (BattleManager.Instance.CurrentState != BattleManager.State.Finish)
        {
            if (state == State.Dead)
                yield break;

            SwitchStateTo(State.Idle);
        }
    }

    private IEnumerator IEnumCheckOwnDeath()
    {
        yield return new WaitForSeconds(3f);

        if (BattleManager.Instance.CurrentState != BattleManager.State.Finish)
        {
            //BattleManager.Instance.ChangeTargetGoal();
            Revive();
        }
    }

    public void PlayAlmostFall()
    {
        if (state == State.Dead)
            return;

        if (!onHit)
            return;

        skeleRoost.AnimationState.SetAnimation(0, "almost fall", true);
        StartCoroutine(IEnumRestoreAlmostFall());
    }

    public void RestoreHit()
    {
        onHit = false;
        if (state == State.Dead)
            return;
        
        anim.Play("idle");
        SwitchStateTo(State.Idle);
    }

    public void DoDamage(float damage, FSMMotor hitter = null)
    {
        if (state == State.Dead)
            return;

        if (onDodge)
            return;

        if (onHit)
            return;

        if (vfxHit.isPlaying)
            vfxHit.Stop();
        vfxHit.Play();

        onHit = true;
        onAction = false;
        cacheAction = null;
        anim.Play("hit" + currentHit);
        currentHit++;
        if (currentHit >= 4)
            currentHit = 0;
        body2D.linearVelocity = Vector2.zero;
        if (customAct != null)
        {
            if (customAct.active)
                customAct.Kill();
        }
        StopAllCoroutines();
        Vector2 enemyDirection = new Vector2();
        if (hitter != null)
        {
            enemyDirection = ((currentTarget != null) ? currentTarget.transform.position : transform.position) - transform.position;
            enemyDirection = enemyDirection.normalized;

            Quaternion targetRotation = Quaternion.LookRotation(transform.forward, enemyDirection);
            body2D.SetRotation(targetRotation);
        }
        if (UIGameplay.Instance.RotateBased <= 0f && BattleManager.Instance.AvatarGoal == null)
        {
            body2D.linearVelocity = Vector2.zero;
            SetToVictim(true);
            SetToDead();
            if (customAct != null)
            {
                if (customAct.active)
                    customAct.Kill();
            }
            //body2D.DOMove(BattleManager.Instance.TargetTransGoal.position, 0.6f);
            Vector2 targetPos = BattleManager.Instance.TargetTransGoal.position;
            Vector2 selfPos = transform.position;
            body2D.AddForce((targetPos - selfPos).normalized * 150f);
            StartCoroutine(IEnumCheckOwnDeath());
            return;
        }
        else
        {
            body2D.AddForce(enemyDirection * -2f, ForceMode2D.Impulse);
        }
    }

    public void Revive()
    {
        currentHp = maxHp;

        onAction = false;
        onHit = false;
        onDodge = false;
        anim.Play("idle");
        SwitchStateTo(State.Idle);
        SetToVictim(false);
        body2D.linearVelocity = Vector2.zero;

        for (int i = 0; i < availableActions.Count; i++)
        {
            if (availableActions[i] == null)
                continue;

            availableActions[i].Cooldown = 0f;
        }

        BattleManager.Instance.TakeGoal = true;
    }

    public void SetToDead()
    {
        SwitchStateTo(State.Dead);
    }

    public void SetToDeadVelocity()
    {
        body2D.angularVelocity = 0f;
        body2D.linearVelocity = new Vector2(0f, 0f);

        skeleRoost.AnimationState.SetAnimation(0, "die", true);
        animationListener.PlayAudio("die01");

        StopAllCoroutines();
    }

    public void ResetMotor()
    {
        currentHp = maxHp;

        onAction = false;
        onHit = false;
        onDodge = false;
        anim.Play("idle");
        SwitchStateTo(State.Idle);
        SetToVictim(false);
        body2D.linearVelocity = Vector2.zero;
        transform.position = positionAwake;
        transform.eulerAngles = eulerAwake;

        for (int i = 0; i < availableActions.Count; i++)
        {
            if (availableActions[i] == null)
                continue;

            availableActions[i].Cooldown = 0f;
        }
    }
}

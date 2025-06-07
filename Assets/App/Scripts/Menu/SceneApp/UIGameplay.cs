using DG.Tweening;
using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using static UnityEditor.Progress;
using TMPro;
using Spine.Unity;

public class UIGameplay : UIPage
{
    [System.Serializable]
    public class GroupTableCell
    {
        [SerializeField] protected string aliasKey = "";
        public string AliasKey
        {
            get
            {
                return aliasKey;
            }
        }

        [SerializeField] UIButtonCell buttonCell = null;
        public UIButtonCell ButtonCell
        {
            get
            {
                return buttonCell;
            }
        }

        public GroupTableCell()
        {
            aliasKey = "";
            buttonCell = null;
        }

        public void DoMatchOfOutput(int output)
        {
            bool doMatch = false;
            List<int> availableGroup = StringUtility.ConvertStringToIntList(aliasKey);
            for (int i = 0; i < availableGroup.Count; i++)
            {
                if (availableGroup[i] == output)
                {
                    doMatch = true;
                    break;
                }
            }

            buttonCell.SetHighlight(doMatch);
        }
    }

    [System.Serializable]
    public class UITable
    {
        [SerializeField] protected TMP_Text textBetAmount = null;
        [SerializeField] protected CanvasGroup canvasGroup = null;
        public CanvasGroup CanvaGroup { get { return canvasGroup; } }

        [SerializeField] protected GameObject parentCells = null;
        [SerializeField] protected List<GroupTableCell> availableGroup = new List<GroupTableCell>();
        protected List<UIButtonCell> availableCells = new List<UIButtonCell>();

        public UITable() {
            parentCells = null;
            availableCells = new List<UIButtonCell>();
        }

        public void SetEnable(bool state)
        {
            canvasGroup.interactable = state;
            canvasGroup.blocksRaycasts = state;
        }

        public void Init()
        {
            UIButtonCell[] tmpCells = parentCells.GetComponentsInChildren<UIButtonCell>();
            availableCells = tmpCells.ToList();
        }

        public void ClearChipsOnTable()
        {
            foreach(UIButtonCell cell in availableCells)
            {
                if (cell == null)
                    continue;

                cell.CacheBet = null;
            }

            textBetAmount.text = "IDR 0";
        }

        public void RenderChipsOnTable(List<SerializeBet> rawBets)
        {
            ClearChipsOnTable();

            var grouped = rawBets
            .GroupBy(x => x.idCell)
            .Select(g => new SerializeBet
            {
                idCell = g.Key,
                amount = g.Sum(x => x.amount)
            }).ToList();

            float totalBet = 0;
            for (int i = 0; i < grouped.Count; i++)
            {
                if (grouped[i] == null)
                    continue;

                UIButtonCell fetchCell = availableCells.Where(x => x.IdCell == grouped[i].idCell).FirstOrDefault();
                fetchCell.CacheBet = new SerializeBet
                {
                    idCell = grouped[i].idCell,
                    amount = grouped[i].amount,
                };
                totalBet += (float) fetchCell.CacheBet.amount;
            }

            textBetAmount.text = StringUtility.ConvertDoubleToStringCurrency(totalBet);
        }

        public void SetHighlight(bool state, int output)
        {
            if (!state)
            {
                foreach (UIButtonCell cell in availableCells)
                {
                    if (cell == null)
                        continue;

                    cell.SetHighlight(false);
                }
                return;
            }

            string findCode = output.ToString();

            UIButtonCell fetchCell = availableCells.Where(x => x.IdCell == findCode).FirstOrDefault();
            fetchCell.SetHighlight(true);

            foreach (GroupTableCell group in availableGroup)
            {
                if (group == null)
                    continue;

                group.DoMatchOfOutput(output);
            }
        }
    }
    public static UIGameplay Instance { get; private set; }

    [SerializeField] protected AudioSource sfxRingBell = null;

    [SerializeField] protected SkeletonGraphic skeleJumbotron = null;
    [SerializeField] protected SkeletonGraphic skeleCoinBurst = null;
    [SerializeField] protected CanvasGroup bgBet = null;
    [SerializeField] protected Transform rouletteWheel = null;
    [SerializeField] protected TMP_Text textResult = null;
    [SerializeField] protected Transform transTextReward = null;
    [SerializeField] protected TMP_Text textReward = null;
    [SerializeField] protected Button btnPlay = null;

    [Header("Player Info")]
    [SerializeField] protected TMP_Text textBalance = null;
    protected UserDataResponse cacheUser = null;
    public UserDataResponse CacheUser
    {
        get
        {
            return cacheUser;
        }
        set
        {
            cacheUser = value;

            textBalance.text = StringUtility.ConvertDoubleToStringCurrency(cacheUser.data.player.player_balance);
        }
    }
    
    [Header("Chip Props")]
    [SerializeField] protected CanvasGroup chipMenuGrp = null;
    [SerializeField] protected List<Sprite> skinChips = new List<Sprite>();
    public List<Sprite> SkinChips
    {
        get
        {
            return skinChips;
        }
    }

    [SerializeField] protected List<float> availableChipAmount = new List<float>();
    [SerializeField] protected List<UIChipButton> availableChips = new List<UIChipButton>();
    public List<UIChipButton> AvailableChips
    {
        get
        {
            return availableChips;
        }
    }


    [Header("Table Props")]
    [SerializeField] protected UITable table = new UITable();
    [SerializeField] protected List<SerializeBet> serializeBets = new List<SerializeBet>();

    protected List<SerializeBet> lastBets = new List<SerializeBet>();

    protected float selectedChip = 500f;

    [Header("Option Access")]
    [SerializeField] protected RectTransform rectoptContainer = null;
    protected bool isOpenOption = false;
    public bool IsOpenOption
    {
        get
        {
            return isOpenOption;
        }
        set
        {
            isOpenOption = value;
            rectoptContainer.DOKill();
            rectoptContainer.DOAnchorPos(isOpenOption == false ? new Vector2(0f, 725f) : Vector2.zero, 0.45f);
        }
    }

    public void OnClickToggleOptAccess()
    {
        IsOpenOption = !isOpenOption;
    }

    protected float rotateBased = 0f;
    public float RotateBased
    {
        get
        {
            return rotateBased;
        }
    }

    Sequence currSequence = null;

    public void OnReturnAuth(UserDataResponse response)
    {
        CacheUser = response;
    }

    public void SetEnableChipMenuGrp(bool state)
    {
        chipMenuGrp.blocksRaycasts = state;
        chipMenuGrp.interactable = state;
    }

    protected void PasteToCurrentBets()
    {
        serializeBets.Clear();
        for (int i = 0; i < lastBets.Count; i++)
        {
            if (lastBets[i] == null)
                continue;

            SerializeBet cacheBet = new SerializeBet()
            {
                idCell = lastBets[i].idCell,
                amount = lastBets[i].amount,
            };

            serializeBets.Add(cacheBet);
        }
    }

    protected void CopyToLastBets()
    {
        lastBets.Clear();
        for (int i = 0; i < serializeBets.Count; i++) {
            if (serializeBets[i] == null)
                continue;

            SerializeBet cacheBet = new SerializeBet()
            {
                idCell = serializeBets[i].idCell,
                amount = serializeBets[i].amount,
            };

            lastBets.Add(cacheBet);
        }
    }

    public void ClearAllChip()
    {
        serializeBets.Clear();
        table.ClearChipsOnTable();
    }

    public void RebetChip()
    {
        if (lastBets.Count <= 0)
            return;

        PasteToCurrentBets();
        table.RenderChipsOnTable(serializeBets);
    }

    public void InsertChip(string idCell)
    {
        SerializeBet newBet = new SerializeBet();
        newBet.amount = selectedChip;
        newBet.idCell = idCell;

        serializeBets.Add(newBet);
        table.RenderChipsOnTable(serializeBets);

        Debug.Log("Insert chip as " + idCell);
    }

    public void OnBetOpen()
    {
        if (currSequence != null)
        {
            if (currSequence.active)
                currSequence.Kill();
        }
        skeleJumbotron.gameObject.SetActive(true);
        currSequence = DOTween.Sequence();
        currSequence.AppendCallback(() =>
        {
            skeleJumbotron.AnimationState.TimeScale = 2f;
            skeleJumbotron.AnimationState.SetAnimation(0, "Bet", false);
        });
        currSequence.Join(bgBet.DOFade(1f, 0.7f));
        currSequence.AppendInterval(0.25f);
        currSequence.AppendCallback(() => {
            BattleManager.Instance.ResetBattle();

            if (btnPlay != null)
                btnPlay.gameObject.SetActive(true);
        });
    }

    public void OnShowResult()
    {
        if (currSequence != null)
        {
            if (currSequence.active)
                currSequence.Kill();
        }
        textResult.gameObject.SetActive(true);
        int targetGoal = BattleManager.Instance.TargetGoal;
        SpriteRenderer markRenderer = BattleManager.Instance.TargetSpriteGoal;
        textResult.text = BattleManager.Instance.TargetGoal.ToString();
        currSequence = DOTween.Sequence();
        currSequence.Append(markRenderer.DOFade(1f, 0.5f));
        currSequence.Append(markRenderer.DOFade(0f, 0.5f).OnComplete(() =>
        {
            skeleJumbotron.AnimationState.TimeScale = 1.5f;
            skeleJumbotron.AnimationState.SetAnimation(0, "Result", false);
        }));
        currSequence.Append(markRenderer.DOFade(1f, 0.5f));
        currSequence.Append(markRenderer.DOFade(0f, 0.5f));
        currSequence.Append(markRenderer.DOFade(1f, 0.5f));
        currSequence.Append(markRenderer.DOFade(0f, 0.5f));
        currSequence.Append(table.CanvaGroup.DOFade(1f, 0.5f));
        currSequence.AppendCallback(() => {
            table.SetHighlight(true, targetGoal);
        });
        currSequence.AppendInterval(2f);
        currSequence.AppendCallback(() => {
            AudioManager.Instance.Play("sfx", "coinburst", false);
        });
        currSequence.Append(transTextReward.DOScale(1f, 0.45f));
        currSequence.AppendCallback(() => {
            skeleCoinBurst.AnimationState.SetAnimation(0, "Loop", true);
        });
        currSequence.AppendInterval(2f);
        currSequence.Append(transTextReward.DOScale(0f, 0.35f));
        currSequence.AppendCallback(() => {
            textResult.gameObject.SetActive(false);
            skeleCoinBurst.AnimationState.SetAnimation(0, "Loop", false);
            OnFinishReached();
        });
    }

    public void OnFinishReached()
    {
        if (currSequence != null)
        {
            if (currSequence.active)
                currSequence.Kill();
        }
        currSequence = DOTween.Sequence();
        currSequence.AppendInterval(1f);
        currSequence.AppendCallback(() => {
            table.SetHighlight(false, 0);
            ClearAllChip();
        });
        currSequence.AppendCallback(() =>
        {
            skeleJumbotron.AnimationState.TimeScale = 2f;
            skeleJumbotron.AnimationState.SetAnimation(0, "Bet", false);
        });
        currSequence.Join(bgBet.DOFade(1f, 0.7f));
        currSequence.Join(table.CanvaGroup.DOFade(1f, 0.3f));
        currSequence.Join(chipMenuGrp.DOFade(1f, 0.3f));
        currSequence.AppendInterval(0.25f);
        currSequence.AppendCallback(() => {
            BattleManager.Instance.ResetBattle();
            table.SetEnable(true);
            SetEnableChipMenuGrp(true);

            if (btnPlay != null)
                btnPlay.gameObject.SetActive(true);
        });
    }

    public void StopRouletteWheel()
    {
        DOVirtual.Float(1f, 0f, 3f, (floatUpdate) => {
            rotateBased = floatUpdate;
        });
    }

    public void StartRouletteWheel()
    {
        rotateBased = 1f;
    }

    public void OnClickPlay()
    {
        if (BattleManager.Instance.CurrentState == BattleManager.State.Play)
            return;

        if (btnPlay != null)
            btnPlay.gameObject.SetActive(false);

        table.SetEnable(false);
        SetEnableChipMenuGrp(false);

        if (sfxRingBell != null)
            sfxRingBell.Play();

        if (currSequence != null) {
            if (currSequence.active)
                currSequence.Kill();
        }
        currSequence = DOTween.Sequence();
        currSequence.AppendCallback(() =>
        {
            skeleJumbotron.AnimationState.TimeScale = 2.5f;
            skeleJumbotron.AnimationState.SetAnimation(0, "Fight", false);
        });
        currSequence.Join(table.CanvaGroup.DOFade(0f, 0.3f));
        currSequence.Join(chipMenuGrp.DOFade(0f, 0.3f));
        currSequence.AppendInterval(0.25f);
        currSequence.AppendCallback(() => {
            CopyToLastBets();
            BattleManager.Instance.TargetGoal = Random.Range(0, 36);
            BattleManager.Instance.Play();

            StartRouletteWheel();
        });
        bgBet.DOFade(0f, 0.7f);
    }

    public void PlayRingBell()
    {
        if (sfxRingBell != null)
            sfxRingBell.Play();
    }

    public override void SetActive(bool state)
    {
        base.SetActive(state);

        switch (state)
        {
            case true:
                OnBetOpen();
                break;
            default:
                break;
        }
    }

    public void OnSelectChip(float amount)
    {
        selectedChip = amount;

        for (int i = 0; i < availableChips.Count; i++) {
            if (availableChips[i] == null)
                continue;

            availableChips[i].SetStateActive(false);
        }

        UIChipButton selectedMono = availableChips.Where(x => x.AmountChip == selectedChip).FirstOrDefault();
        if (selectedMono != null) {
            selectedMono.SetStateActive(true);
        }
    }

    protected void InitCells()
    {
        serializeBets = new List<SerializeBet>();
        table.Init();
    }

    protected void InitChips()
    {
        for (int i = 0; i < availableChipAmount.Count; i++) {
            availableChips[i].UpdateAmount(availableChipAmount[i]);
        }

        for (int i = 0; i < availableChips.Count; i++)
        {
            availableChips[i].UpdateSprite(skinChips[i]);
        }

        availableChips[0].OnClickChip();
    }

    protected override void Awake()
    {
        base.Awake();

        Instance = this;
    }

    protected override void Start()
    {
        base.Start();

        InitCells();
        InitChips();

        skeleJumbotron.gameObject.SetActive(false);
    }

    protected void FixedUpdate()
    {
        rouletteWheel.Rotate(Vector3.forward * rotateBased * 80f * Time.deltaTime);
    }
}

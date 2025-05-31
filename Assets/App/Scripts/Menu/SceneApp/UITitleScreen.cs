using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UITitleScreen : UIPage
{
    public static UITitleScreen Instance { get; protected set; }

    //[SerializeField] protected SkeletonGraphic titleScreenSpine = null;
    //[SerializeField] protected Image titleLogo = null;
    //[SerializeField] protected RectTransform actionContainer = null;
    //[SerializeField] protected Button btnStart = null;
    //[SerializeField] protected Button btnHistory = null;

    protected Sequence transSequence = null;

    public void OnActiveAccessMenu(bool state)
    {
        //actionContainer.gameObject.SetActive(state);
    }

    public void OpenToGame()
    {
        UIRenderer.Instance.OpenPage(UIGameplay.Instance);
        //titleScreenSpine.AnimationState.SetAnimation(0, "start game", false);
        //AudioManager.Instance.Play("sfx", "confirm");
    }

    public override void SetActive(bool state)
    {
        base.SetActive(state);

        if (state)
        {
            //titleScreenSpine.AnimationState.SetAnimation(0, "start", false);

            //transSequence = DOTween.Sequence();
            //transSequence.Append(actionContainer.DOAnchorPos(new Vector2(0f, 0f), 0.75f));
            //transSequence.Join(titleLogo.DOFade(1f, 0.35f));
        }
    }

    protected override void Awake()
    {
        base.Awake();

        UITitleScreen.Instance = this;
    }

    protected override void Start()
    {
        base.Start();

        //if (titleScreenSpine != null)
        //{

        //    titleScreenSpine.AnimationState.Start += (Spine.TrackEntry entry) =>
        //    {
        //        if (entry == null)
        //            return;

        //        if (entry.Animation == null)
        //            return;

        //        if (entry.Animation.Name == "start game")
        //        {
        //            //transSequence = DOTween.Sequence();
        //            //transSequence.Append(actionContainer.DOAnchorPos(new Vector2(0f, -1000f), 0.75f));
        //            //transSequence.Join(titleLogo.DOFade(0f, 0.35f));
        //        }
        //    };

        //    titleScreenSpine.AnimationState.Complete += (Spine.TrackEntry entry) =>
        //    {
        //        if (entry == null)
        //            return;

        //        if (entry.Animation == null)
        //            return;

        //        if (entry.Animation.Name == "start game")
        //        {
        //            //UIRenderer.Instance.OpenPage(UIGameplay.Instance);
        //        } else if (entry.Animation.Name == "start")
        //        {
        //            titleScreenSpine.AnimationState.SetAnimation(0, "idle", true);
        //        }
        //    };
        //}

        //AudioManager.Instance.Play("bgm", "main", true);
    }
}

using Spine;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIButtonSpine : Button
{
    [SerializeField] protected SkeletonGraphic skeleton = null;

    public void onClickSpineButton()
    {
        if (skeleton != null)
            skeleton.AnimationState.SetAnimation(0, "push button", false);
    }

    protected override void Start()
    {
        base.Start();
        if (skeleton == null)
            return;

        skeleton.AnimationState.Complete += (entry) =>
        {
            if (entry == null)
                return;

            if (entry.Animation == null)
                return;

            if (entry.Animation.Name == "push button")
            {
                skeleton.AnimationState.SetAnimation(0, "idle", true);
            }
        };
    }
}

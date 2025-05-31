using Spine;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXHitSpawnBehaviour : MonoBehaviour
{
    [SerializeField] protected BattleManager battleManager = null;
    [SerializeField] protected SkeletonAnimation vfxAnimation = null;

    public void OnCompleteAnimation(TrackEntry trackEntry)
    {
        if (trackEntry == null)
            return;

        if (trackEntry.Animation == null)
            return;

        if (trackEntry.Animation.Name == "clash")
        {
            battleManager.DespawnVFXHit(this.gameObject);
        }
    }

    private void OnDisable()
    {
        vfxAnimation.AnimationState.Complete -= OnCompleteAnimation;
    }

    // Start is called before the first frame update
    private void OnEnable()
    {
        if (battleManager == null)
            battleManager = BattleManager.Instance;

        vfxAnimation.AnimationState.Complete += OnCompleteAnimation;

        vfxAnimation.AnimationState.SetAnimation(0, "clash", false);
    }
}

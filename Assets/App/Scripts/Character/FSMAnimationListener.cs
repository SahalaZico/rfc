using DG.Tweening;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FSMAnimationListener : MonoBehaviour
{
    [Serializable]
    public class FSMAudioAction
    {
        [SerializeField] private string key = "default";
        [SerializeField] private AudioSource audioSource = null;

        public FSMAudioAction()
        {
            key = "default";
            audioSource = null;
        }

        public string Key
        {
            get
            {
                return key;
            }
        }

        public AudioSource AudioSrc
        {
            get
            {
                return audioSource;
            }
        }
    }

    [SerializeField] protected FSMMotor motor = null;
    [SerializeField] protected FSMDamageCollider damageCollider = null;
    [SerializeField] protected SkeletonAnimation vfxClaw = null;
    [SerializeField] protected SkeletonAnimation vfxClash = null;
    [SerializeField] protected TrailRenderer vfxTrail = null;
    [SerializeField] protected List<FSMAudioAction> audios = new List<FSMAudioAction>();

    protected Material cacheMaterial = null;
    protected MeshRenderer meshRenderer = null;
    protected Sequence sequenceDamage = null;

    public void PlayClash()
    {
        vfxClash.gameObject.SetActive(true);
        vfxClash.AnimationState.SetAnimation(0, "clash", false);
    }

    public void PlayClaw()
    {
        vfxClaw.gameObject.SetActive(true);
        vfxClaw.AnimationState.SetAnimation(0, "claw", false);
    }

    public void PlayAudio(string key)
    {

        FSMAudioAction audioSfx = audios.Where(x => x.Key == key).FirstOrDefault();
        if (audioSfx != null)
        {
            if (audioSfx.AudioSrc.isPlaying)
                audioSfx.AudioSrc.Stop();

            audioSfx.AudioSrc.Play();
        }
    }

    public void PlayAudioAction(string key)
    {
        if (BattleManager.Instance.CurrentState != BattleManager.State.Play)
            return;

        FSMAudioAction audioSfx = audios.Where(x => x.Key == key).FirstOrDefault();
        if (audioSfx != null)
        {
            if (audioSfx.AudioSrc.isPlaying)
                audioSfx.AudioSrc.Stop();

            audioSfx.AudioSrc.Play();
        }
    }

    public void PlayAudioActionWithRandom(string key)
    {
        if (BattleManager.Instance.CurrentState != BattleManager.State.Play)
            return;

        int resultCatch = UnityEngine.Random.Range(0, 10);
        if (resultCatch == 0 || resultCatch == 3 || resultCatch == 9)
        {
            FSMAudioAction audioSfx = audios.Where(x => x.Key == key).FirstOrDefault();
            if (audioSfx != null)
            {
                if (audioSfx.AudioSrc.isPlaying)
                    audioSfx.AudioSrc.Stop();

                audioSfx.AudioSrc.Play();
            }
        }
    }

    public void SetOrderLayer(int orderLayer)
    {
        if (meshRenderer == null)
            meshRenderer = motor.SkeleRoost.gameObject.GetComponent<MeshRenderer>();

        if (meshRenderer != null)
            meshRenderer.sortingOrder = orderLayer;
    }

    public void SetDamageCollider(int state)
    {
        damageCollider.gameObject.SetActive(state > 0);
    }

    public void SetAnimation(string clipPlay)
    {
        motor.SkeleRoost.AnimationState.SetAnimation(0, clipPlay, false);

        if (clipPlay == "hit")
        {
            vfxTrail.gameObject.SetActive(true);
            if (sequenceDamage != null)
            {
                if (sequenceDamage.active)
                    sequenceDamage.Kill();

                sequenceDamage = null;
            }

            sequenceDamage = DOTween.Sequence();
            sequenceDamage.AppendCallback(() =>
            {
                motor.SkeleRoost.skeleton.SetColor(Color.red);
            });
            sequenceDamage.AppendInterval(0.3f);
            sequenceDamage.AppendCallback(() =>
            {
                motor.SkeleRoost.skeleton.SetColor(Color.white);
            });
            sequenceDamage.AppendInterval(0.3f);
            sequenceDamage.SetLoops(-1);
        }
    }

    public void RestoreHit()
    {
        motor.RestoreHit();

        vfxTrail.gameObject.SetActive(false);
        if (sequenceDamage != null)
        {
            if (sequenceDamage.active)
                sequenceDamage.Kill();

            sequenceDamage = null;
        }

        motor.SkeleRoost.skeleton.SetColor(Color.white);
    }

    public void DisableOnAction()
    {
        if (motor == null)
            return;

        motor.OnAction = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        vfxClaw.AnimationState.Complete += (trackEntry) =>
        {
            if (trackEntry == null)
                return;

            if (trackEntry.Animation == null)
                return;

            if (trackEntry.Animation.Name == "claw")
            {
                vfxClaw.gameObject.SetActive(false);
            }
        };

        vfxClash.AnimationState.Complete += (trackEntry) =>
        {
            if (trackEntry == null)
                return;

            if (trackEntry.Animation == null)
                return;

            if (trackEntry.Animation.Name == "clash")
            {
                vfxClash.gameObject.SetActive(false);
            }
        };

        vfxClash.gameObject.SetActive(false);
        vfxClaw.gameObject.SetActive(false);
    }
}

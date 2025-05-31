using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIButtonCell : Button
{
    [SerializeField] private string idCell = "";
    public string IdCell
    {
        get
        {
            return idCell;
        }
    }

    [SerializeField] private GameObject gameChip = null;
    [SerializeField] private Image imageChip = null;
    [SerializeField] private Image imageHighlight = null;
    [SerializeField] private TMP_Text textChip = null;

    protected Sequence highlightSequence = null;

    private SerializeBet cacheBet = null;
    public SerializeBet CacheBet
    {
        get
        {
            return cacheBet;
        }
        set
        {
            cacheBet = value;
            if (cacheBet == null)
            {
                textChip.text = "0";
                gameChip.SetActive(false);
                return;
            }
            int getIndex = 0;
            List<UIChipButton> chips = UIGameplay.Instance.AvailableChips;
            List<Sprite> skins = UIGameplay.Instance.SkinChips;
            for (int i = 0; i < chips.Count; i++) {
                if (chips[i] == null)
                    continue;

                if (i >= skins.Count)
                {
                    getIndex = skins.Count - 1;
                    break;
                }

                if (cacheBet.amount >= chips[i].AmountChip)
                    getIndex = i;
                else
                    break;
            }
            imageChip.sprite = skins[getIndex];
            gameChip.SetActive(true);
            textChip.text = StringUtility.ConvertToFormatNumber((float) cacheBet.amount);
        }
    }
    
    public void SetHighlight(bool state)
    {
        imageHighlight.gameObject.SetActive(state);

        switch (state) {
            case true:
                if (highlightSequence != null)
                {
                    if (highlightSequence.IsActive())
                        highlightSequence.Kill();
                }
                Color fetchColor = imageHighlight.color;
                fetchColor.a = 1f;
                imageHighlight.color = fetchColor;
                highlightSequence = DOTween.Sequence();
                highlightSequence.Append(imageHighlight.DOFade(0f, 0.4f));
                highlightSequence.Append(imageHighlight.DOFade(1f, 0.4f));
                highlightSequence.SetLoops(-1);
                break;
            default:
                if (highlightSequence != null)
                {
                    if (highlightSequence.IsActive())
                        highlightSequence.Kill();
                }
                Color fetchColorDeactive = imageHighlight.color;
                fetchColorDeactive.a = 0f;
                imageHighlight.color = fetchColorDeactive;
                break;
        }
    }

    public void OnClickCell()
    {
        UIGameplay.Instance.InsertChip(idCell);
    }
}

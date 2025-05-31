using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIChipButton : Button
{
    [SerializeField] private float amountChip = 500f;
    public float AmountChip
    {
        get
        {
            return amountChip;
        }
    }

    [SerializeField] private GameObject gameChip = null;
    [SerializeField] private GameObject selectedChip = null;
    [SerializeField] private Image skinChip = null;
    [SerializeField] private TMP_Text textChip = null;

    public void SetStateActive(bool state)
    {
        selectedChip.SetActive(state);
    }

    public void OnClickChip()
    {
        UIGameplay.Instance.OnSelectChip(amountChip);
    }

    public void UpdateAmount(float inputAmount)
    {
        amountChip = inputAmount;
        textChip.text = StringUtility.ConvertToFormatNumber(amountChip);
    }

    public void UpdateSprite(Sprite passedSprite)
    {
        skinChip.sprite = passedSprite;
    }

    protected override void OnValidate()
    {
        base.OnValidate();

        if (textChip == null)
            return;

        textChip.text = StringUtility.ConvertToFormatNumber(amountChip);
    }
}

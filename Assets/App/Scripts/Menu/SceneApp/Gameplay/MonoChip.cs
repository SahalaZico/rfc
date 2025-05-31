using UnityEngine;

[System.Serializable]
public class SerializeChip
{
    [SerializeField] private Sprite sprite = null;

    public SerializeChip()
    {
        sprite = null;
    }
}

[System.Serializable]
public class SerializeBet
{
    public string idCell = "";
    public double amount = 0f;

    public SerializeBet() {
        idCell = "";
        amount = 0f;
    }
}

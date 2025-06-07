using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance { get; protected set; }

    protected UserDataResponse user = null;

    [SerializeField]
    protected UnityEvent<UserDataResponse> OnReturnAuthData;

    public UserDataResponse User
    {
        get
        {
            return user;
        }
    }

    public string GetUserCurrency()
    {
        if (user == null)
            return "";

        return user.data.player.player_currency;
    }

    public void SetUserData(UserDataResponse input)
    {
        user = input;
        OnReturnAuthData?.Invoke(user);
    }

    void Awake()
    {
        PlayerData.Instance = this;
    }
}

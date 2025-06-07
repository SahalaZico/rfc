using Newtonsoft.Json;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;
using AnomalyLab.Backend;

public class CryptoCS: MonoBehaviour
{
    [System.Serializable]
    public class DataParse
    {
        public string data = "";

        public DataParse() {
            data = "";
        }
    }

    [System.Serializable]
    public class EncryptRoot
    {
        public string encrypted_data { get; set; }
    }

    [System.Serializable]
    public class DecryptRoot
    {
        public string decrypted_data { get; set; }
    }

    public static CryptoCS Instance { get; protected set; }

    protected APIManager apiManager = null;
    [SerializeField] protected string key = "";
    [Header("Encrypt URL")]
    [SerializeField] protected string linkEncrypt = "/game/enc-result";
    [Header("Encrypt URL")]
    [SerializeField] protected string linkDecrypt = "/game/dec-result";

    protected System.Action<bool, string> OnReturnEncryptCallback = null;
    protected System.Action<bool, string> OnReturnDecryptCallback = null;

    [DllImport("__Internal")]
    private static extern string EncryptDataAndSendBack(string data, string key);

    [DllImport("__Internal")]
    private static extern string DecryptDataAndSendBack(string data, string key);

    protected void Awake()
    {
        Instance = this;
    }

    protected void Start()
    {
        apiManager = GetComponent<APIManager>();
    }

    public void ReceiveEncryptedData(string encryptedData)
    {
        EncryptRoot encryptBody = new EncryptRoot()
        {
            encrypted_data = encryptedData
        };
        string encryptedJsonStr = JsonConvert.SerializeObject(encryptBody);
        Debug.Log("EncryptJS Parsed: " + encryptedJsonStr);
        OnReturnEncryptCallback?.Invoke(true, encryptedJsonStr);
    }

    public void ReceiveDecryptedData(string decryptedData)
    {
        try
        {
            DecryptRoot decryptBody = new DecryptRoot()
            {
                decrypted_data = decryptedData
            };
            string decryptedJsonStr = JsonConvert.SerializeObject(decryptBody);
            Debug.Log("DecryptJS Parsed: " + decryptedJsonStr);
            OnReturnDecryptCallback?.Invoke(true, decryptedJsonStr);
        } catch (System.Exception err)
        {
            Debug.Log("ReceiveDecrypt err: " + err.Message);
        }
    }

    protected IEnumerator IEEncryptTask(DataParse data, string key, System.Action<bool, string> onReturn = null)
    {
#if UNITY_EDITOR
        string dataJsonStr = JsonConvert.SerializeObject(data, new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.Default });
        using (UnityWebRequest www = UnityWebRequest.Post(apiManager.EnvCache.game + linkEncrypt, dataJsonStr, "application/json"))
        {
            www.downloadHandler = new DownloadHandlerBuffer();
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                onReturn?.Invoke(false, "");
                Debug.LogError(www.error);
            }
            else
            {
                var resultText = www.downloadHandler.text;
                onReturn?.Invoke(true, resultText);
                Debug.Log(resultText);
            }
        }
#else
        OnReturnEncryptCallback = onReturn;
        string encRes = EncryptDataAndSendBack(data.data, this.key);
        ReceiveEncryptedData(encRes);
#endif
        yield return null;
    }

    public void EncryptAESWithECB(DataParse data, string key, System.Action<bool, string> onReturn = null)
    {
        StartCoroutine(IEEncryptTask(data, key, onReturn));
    }

    protected IEnumerator IEDecryptTask(DataParse data, string key, System.Action<bool, string> onReturn = null)
    {
#if UNITY_EDITOR
        string dataJsonStr = JsonConvert.SerializeObject(data, new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.Default });
        using (UnityWebRequest www = UnityWebRequest.Post(apiManager.EnvCache.game + linkDecrypt, dataJsonStr, "application/json"))
        {
            www.downloadHandler = new DownloadHandlerBuffer();
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                onReturn?.Invoke(false, "");
                Debug.LogError(www.error);
            }
            else
            {
                var resultText = www.downloadHandler.text;
                onReturn?.Invoke(true, resultText);
                Debug.Log(resultText);
            }
        }
#else
        OnReturnDecryptCallback = onReturn;
        string decRes = DecryptDataAndSendBack(data.data, this.key);
        ReceiveDecryptedData(decRes);
#endif
        yield return null;
    }

    public void DecryptAESWithECB(DataParse data, string key, System.Action<bool, string> onReturn = null)
    {
        try
        {
            StartCoroutine(IEDecryptTask(data, key, onReturn));
        } catch (System.Exception err)
        {
            Debug.Log("Decrypt err: " + err.Message);
        }
    }
}

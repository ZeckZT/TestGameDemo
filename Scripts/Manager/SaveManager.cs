using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : Singleton<SaveManager>
{
    string sceneName="";

    public string SceneName{get{return PlayerPrefs.GetString(sceneName);}}
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }


    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            SceneController.Instance.TransitionToMain();
        }
    }
    public void SavaPlayerData()
    {
        Save(GameManager.Instance.playerStats.characterData, GameManager.Instance.playerStats.characterData.name);
    }
    public void LoadPlayerData()
    {
        Load(GameManager.Instance.playerStats.characterData, GameManager.Instance.playerStats.characterData.name);
    }
    public void Save(object data, string key)
    {
        var JsonData = JsonUtility.ToJson(data, true);
        PlayerPrefs.SetString(key, JsonData);
        PlayerPrefs.Save();
        PlayerPrefs.SetString(sceneName, SceneManager.GetActiveScene().name);
    }

    public void Load(object data, string key)
    {
        if(PlayerPrefs.HasKey(key))
        {
            JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString(key), data);


        }
    }
}

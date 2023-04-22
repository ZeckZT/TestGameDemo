using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

public class SceneController : Singleton<SceneController>, IEndGameObserver
{
    public GameObject PlayerPrefab;
    public SceneFade sceneFadePrefab;

    bool fadeFinish;
    GameObject Player;
    NavMeshAgent playerAgent;
    
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    void Start()
    {
        GameManager.Instance.AddObserver(this);
        fadeFinish = true;
    }
    public void TransitionDestination(TransitionPoint transitionPoint)
    {
        switch (transitionPoint.transitionType)
        {
            case TransitionPoint.TransitionType.SameScene:
            StartCoroutine(Transition(SceneManager.GetActiveScene().name,transitionPoint.destinationTag));
                break;
            case TransitionPoint.TransitionType.DifferentScene:
            StartCoroutine(Transition(transitionPoint.sceneName, transitionPoint.destinationTag));
                break;
        }
    }

    IEnumerator Transition(string sceneName, TransitionDestination.DestinationTag destinationTag)
    {
        //保存数据
        SaveManager.Instance.SavaPlayerData();
        if(SceneManager.GetActiveScene().name != sceneName)
        {
            yield return SceneManager.LoadSceneAsync(sceneName);
            yield return Instantiate(PlayerPrefab, GetDestination(destinationTag).transform.position, GetDestination(destinationTag).transform.rotation);
            SaveManager.Instance.LoadPlayerData();
            yield break;
        }
        else
        {
            Player = GameManager.Instance.playerStats.gameObject;
            playerAgent = Player.GetComponent<NavMeshAgent>();
            playerAgent.enabled = false;
            Player.transform.SetPositionAndRotation(GetDestination(destinationTag).transform.position, GetDestination(destinationTag).transform.rotation);
            playerAgent.enabled = true;
            yield return null;
        }

    }

    private TransitionDestination GetDestination(TransitionDestination.DestinationTag destinationTag)
    {
        var entrances = FindObjectsOfType<TransitionDestination>();

        for (int i = 0; i < entrances.Length; i++)
        {
            if(entrances[i].destinationTag == destinationTag)
                return entrances[i];
        }



        return null;
    }

    public void TransitionToMain()
    {
        StartCoroutine(LoadMain());
    }

    public void TransitionToLoadGame()
    {
        StartCoroutine(LoadScene(SaveManager.Instance.SceneName));
    }
    public void TransitionToFirstLevel()
    {
        StartCoroutine(LoadScene("SampleScene"));
    }
    
    IEnumerator LoadScene(string scene)
    {
        SceneFade fade = Instantiate(sceneFadePrefab);
        if(scene != "")
        {
            yield return StartCoroutine(fade.FadeOut(2.5f));
            yield return SceneManager.LoadSceneAsync(scene);
            yield return Player = Instantiate(PlayerPrefab,GameManager.Instance.GetEntrance().position,GameManager.Instance.GetEntrance().rotation);

            SaveManager.Instance.SavaPlayerData();
            yield return StartCoroutine(fade.FadeIn(2.5f));
            yield break ;
        }
    }

    IEnumerator LoadMain()
    {
        SceneFade fade = Instantiate(sceneFadePrefab);
        yield return StartCoroutine(fade.FadeOut(2.5f));
        yield return SceneManager.LoadSceneAsync("Main");
        yield return StartCoroutine(fade.FadeIn(2.5f));
        yield break;
    }

    void IEndGameObserver.EndNotify()
    {
        if(fadeFinish)
        {
            fadeFinish = false;
            StartCoroutine(LoadMain());
        }
        
    }
}

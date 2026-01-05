
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }
    public Animator transition;
    private void Awake()
    {
        // 防止重复创建实例
        if (Instance != null && Instance != this)
        {
            // 如果已存在的实例仍然有效，销毁当前对象
            Destroy(gameObject);
            return;
        }

        // 如果 Instance 已经等于 this，说明之前已经初始化过了，直接返回
        // 这可以避免重复调用 DontDestroyOnLoad
        if (Instance == this)
        {
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartBattle()
    {
        StartCoroutine(this.LoadBattleScene());
    }

    public IEnumerator LoadBattleScene()
    {
        PlayFadeInAnimation();
        yield return new WaitForSeconds(3f);
        SceneManager.LoadSceneAsync("BattleScene", LoadSceneMode.Additive);

        PlayFadeOutAnimation();

    }

    //卸载战斗场景
    public void UnLoadBattleScene()
    {
        SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName("BattleScene"),UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
    }

    //激活场景
    public void SetActiveSceneByName(string sceneName)
    {
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
    }

    public void PlayFadeInAnimation()
    {
        transition.SetTrigger("FadeIn");
    }
    public void PlayFadeOutAnimation()
    {
        transition.SetTrigger("FadeOut");
    }
}

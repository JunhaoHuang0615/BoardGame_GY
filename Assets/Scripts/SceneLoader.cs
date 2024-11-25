
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }
    public Animator transition;
    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);
    }

    public void StartBattle()
    {
        StartCoroutine(this.LoadBattleScene());
    }

    public IEnumerator LoadBattleScene()
    {
        PlayFadeInAnimation();
        yield return new WaitForSeconds(3f);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("BattleScene", LoadSceneMode.Additive);
        //SceneManager.LoadSceneAsync("BattleScene", LoadSceneMode.Additive);
        // 等待加载完成
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        PlayFadeOutAnimation();
        
    }

    public Scene GetSceneByName(string sceneName)
    {
        return SceneManager.GetSceneByName(sceneName);
    }

    public void MoveGameObjectToTargetScene(GameObject go,Scene targetScene)
    {
        SceneManager.MoveGameObjectToScene(go, targetScene);
    }
    public Canvas FindOrCreateCanvasInScene(Scene targetScene)
    {
        // 切换到目标场景进行查找
        foreach (GameObject rootObject in targetScene.GetRootGameObjects())
        {
            Canvas canvas = rootObject.GetComponentInChildren<Canvas>();
            if (canvas != null)
            {
                return canvas;
            }
        }

        // 没有找到Canvas，创建一个新的Canvas并设置到目标场景中
        GameObject newCanvasObj = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas newCanvas = newCanvasObj.GetComponent<Canvas>();
        newCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

        // 将新的Canvas添加到目标场景
        SceneManager.MoveGameObjectToScene(newCanvasObj, targetScene);

        return newCanvas;
    }

    //卸载战斗场景
    public Task UnLoadBattleSceneAsync()
    {
        var tcs = new TaskCompletionSource<bool>();
        var asyncOp = SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName("BattleScene"), UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);

        if (asyncOp != null)
        {
            asyncOp.completed += (operation) =>
            {
                Debug.Log("BattleScene has been unloaded.");
                tcs.SetResult(true);
            };
        }
        else
        {
            Debug.LogError("Failed to unload the BattleScene.");
            tcs.SetResult(false);
        }
        //可以根据 var task = UnLoadBattleSceneAsync();  task.IsCompleted
        return tcs.Task;
    }
    public void UnLoadBattleScene(Action onComplete = null)
    {
        var asyncOp = SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName("BattleScene"), UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
        if (asyncOp != null)
        {
            asyncOp.completed += (operation) =>
            {
                Debug.Log("BattleScene has been unloaded.");
                onComplete?.Invoke();
            };
        }
        else
        {
            Debug.LogError("Failed to unload the BattleScene. Ensure the scene name is correct and the scene is loaded.");
        }
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

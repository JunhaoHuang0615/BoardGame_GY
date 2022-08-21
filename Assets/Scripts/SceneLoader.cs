
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{   
    public Animator transition;
    private void Awake()
    {
        DontDestroyOnLoad(this);
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingController : MonoBehaviour
{
    public enum SceneIndexes
    {
        LoadingScene = 0,
        MainMenuScene = 1,
        CombatScene = 2,
        EndScene = 3,
        OverworldScene = 4,
        ShopScene = 5,
        ShrineScene = 6,
        TavernScene = 7,
        SettingsScene = 8,
        StoryModeScene = 9,
        StoryModeEndScene = 10,
        StoryModeShopScene = 11,
        StoryModeSecretShopScene = 12,
        PatchNotesScene = 13
    }

    public static LoadingController load;
    public GameObject loadingScene;
    public Image progressBar;

    private List<AsyncOperation> loadOperations = new List<AsyncOperation>();
    private string currentSceneName = "";

    // Start is called before the first frame update
    void Start()
    {
        if (load == null)
            load = this;
        else
            Destroy(this.gameObject);

        LoadScene("MainMenuScene");
    }

    public void LoadScene(string newSceneName)
    {
        loadingScene.SetActive(true);
        if (currentSceneName != "")
            loadOperations.Add(SceneManager.UnloadSceneAsync(currentSceneName));
        loadOperations.Add(SceneManager.LoadSceneAsync(newSceneName, LoadSceneMode.Additive));
        currentSceneName = newSceneName;

        StartCoroutine(GetSceneLoadProgress());
    }

    public IEnumerator GetSceneLoadProgress()
    {
        float progress = 0;
        for (int i = 0; i < loadOperations.Count; i++)
            while (!loadOperations[i].isDone)
            {
                foreach (AsyncOperation operation in loadOperations)
                    progress += operation.progress;

                progress = progress / (float)loadOperations.Count;

                Debug.Log(progress);

                progressBar.transform.localScale = new Vector3(1, progress, 1);

                yield return null;
            }

        loadOperations = new List<AsyncOperation>();

        loadingScene.SetActive(false);
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;



public class MySceneManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // （デバッグ時用）Editor でロードしていた ManagerScene 以外のシーンを消去
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            string sceneName = SceneManager.GetSceneAt(i).name;
            if (sceneName != "ManagerScene")
                SceneManager.UnloadSceneAsync(sceneName);
        }

        // ロードが終了する前にアクティブ化しようとしてしまうので、ロード時に直接アクティブ化せず、イベントハンドラで処理
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene("TitleScene", LoadSceneMode.Additive);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void ChangeSceneRequest(string targetScene)
    {
        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.name != "ManagerScene")
            SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        SceneManager.LoadScene(targetScene, LoadSceneMode.Additive);
    }

    public void OnSceneLoaded(Scene nextScene, LoadSceneMode mode)
    {
        /// EventSystem は常に一つに保つ
        /// まず前のシーンのものを見つけて Deactivate し、次のシーンの オブジェクト EventSystem（コンポーネント EventSystem がついている）を見つけて Activate
        DeactivateCurrentEventSystem();
        Debug.Log($"{nextScene.name} is loaded.");
        SceneManager.SetActiveScene(nextScene);
        GameObject[] nextSceneGameObjects = nextScene.GetRootGameObjects();
        foreach(GameObject ob in nextSceneGameObjects)
        {
            if (ob.GetComponent<EventSystem>() != null)
            {
                ob.SetActive(true);
                EventSystem.current = ob.GetComponent<EventSystem>();
            }
        }

    }

    public void DeactivateCurrentEventSystem()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        GameObject[] currentSceneGameObjects = currentScene.GetRootGameObjects();
        foreach (GameObject ob in currentSceneGameObjects)
        {
            if (ob.GetComponent<EventSystem>() != null)
            {
                ob.SetActive(false);
            }
        }


    }
}

using UnityEngine;
using UnityEngine.EventSystems;

public class TitleManager : MonoBehaviour
{

    private void Awake()
    {
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void OnKeybindingStartButtonClick()
    {
        Debug.Log("pressed keybinding");
        MySceneManager.ChangeSceneRequest("KeybindingScene");
    }
    public void OnGameMainStartButtonClick()
    {
        Debug.Log("pressed gamemain");
        MySceneManager.ChangeSceneRequest("GameMainScene");
    }
}

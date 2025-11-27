using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        // 加载战斗场景 (确保Build Settings里战斗场景索引是1)
        SceneManager.LoadScene(1); 
    }

    // 这段代码可以先留着，等以后发布电脑版(PC/Mac)时再把按钮加回来
    /*
    public void QuitGame()
    {
        Application.Quit();
    }
    */
}
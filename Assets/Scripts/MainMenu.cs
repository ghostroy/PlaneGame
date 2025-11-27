using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        
        SceneManager.LoadScene("GameScene"); 
    }

    // 这段代码可以先留着，等以后发布电脑版(PC/Mac)时再把按钮加回来
    /*
    public void QuitGame()
    {
        Application.Quit();
    }
    */
}
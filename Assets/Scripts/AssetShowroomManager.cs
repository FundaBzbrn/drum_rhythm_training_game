using UnityEngine;
using UnityEngine.SceneManagement;

public class AssetShowroomManager : MonoBehaviour
{
    // Ana menü 
    public string mainMenuSceneName = "MainMenuManager"; 

    public void GoToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
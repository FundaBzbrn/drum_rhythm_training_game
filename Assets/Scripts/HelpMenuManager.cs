
using UnityEngine.SceneManagement; 
using UnityEngine;
public class HelpMenuManager : MonoBehaviour
{
    // Ana Menü 
    public string mainMenuSceneName = "MainMenuManager"; 

    // Geri Butonu
    public void GoToMainMenu()
    {
        Debug.Log(mainMenuSceneName + " sahnesine geri dönülüyor...");
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
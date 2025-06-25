using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsMenuManager : MonoBehaviour
{
    // Ana Menü
    public string MainMenuManager = "MainMenuManager";

    public void GoToMainMenu()
    {
        Debug.Log(MainMenuManager + " sahnesine geri dönülüyor...");
        SceneManager.LoadScene(MainMenuManager);
    }
}
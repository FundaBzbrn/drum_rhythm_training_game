
using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenuManager : MonoBehaviour
{
    // Sahneyi yükleyecek fonksiyon
    public void StartGame()

    {
        GameManager.startLevelIndex = 0; // Her zaman Level 1'den başla
        string gameSceneName = "SampleScene"; // Oyun sahnesi

        Debug.Log(gameSceneName + " sahnesi yükleniyor..."); 
        SceneManager.LoadScene(gameSceneName);
    }


    // Help sahnesini yükleyecek fonksiyon
    public void OpenHelp()
    {

        string helpSceneName = "HelpScene"; 
        Debug.Log(helpSceneName + " sahnesi yükleniyor...");

        // HelpScene'i yükle
        SceneManager.LoadScene(helpSceneName);
    }

    // Options sahnesini yükleyecek fonksiyon
    public void OpenOptions()
    {
        string optionsSceneName = "OptionsScene"; 

        Debug.Log(optionsSceneName + " sahnesi yükleniyor..."); 
        SceneManager.LoadScene(optionsSceneName);
    }

    // Credits sahnesini yükleyecek fonksiyon
    public void OpenCredits()
    {
        string creditsSceneName = "CreditsScene";

        Debug.Log(creditsSceneName + " sahnesi yükleniyor...");
        SceneManager.LoadScene(creditsSceneName);
    }

    // AssetShowroom sahnesini yükleyecek fonksiyon
    public void OpenAssetShowroom() 
    {
        SceneManager.LoadScene("AssetShowroom"); 
    }
}
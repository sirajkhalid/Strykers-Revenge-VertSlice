using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    public void PlayGame()
    {
        SceneManager.LoadScene(3);
    }
    public void CreditGame1()
    {
        SceneManager.LoadScene(11);
    }
    public void TutorialGame()
    {
        SceneManager.LoadScene(2);
    }

    public void QuitGame()
    {
        Debug.Log("QUIT!");
        Application.Quit(); 
    }
}

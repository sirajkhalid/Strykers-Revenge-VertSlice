using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    

    public string sceneName;
    public BoxCollider2D Collider2D;
    PlayerMovement playerMovement;

    public void OnTriggerEnter2D(Collider2D other)
    {
        SceneManager.LoadScene(sceneName);
       
    }

}

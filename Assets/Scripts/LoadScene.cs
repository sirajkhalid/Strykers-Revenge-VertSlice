using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    

    public string sceneName;
    public BoxCollider2D Collider2D;

    public void OnTriggerEnter2D(Collider2D other)
    {
        SceneManager.LoadScene(sceneName);
    }

}

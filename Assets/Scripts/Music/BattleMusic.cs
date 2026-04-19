using UnityEngine;

public class BattleMusic : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip battleMusic, defaultMusic;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        defaultMusic = audioSource.clip;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            audioSource.clip = battleMusic;
            audioSource.Play();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            audioSource.clip = defaultMusic;
            audioSource.Play();
        }
    }
}

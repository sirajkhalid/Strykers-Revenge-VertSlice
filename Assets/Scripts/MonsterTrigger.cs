using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MonsterTrigger : MonoBehaviour
{
    public GameObject Door;
    public GameObject[] arrays;

    public void OnTriggerExit2D(Collider2D other)
    {
        Door.SetActive(true);
        for (int i = 0; i < arrays.Length; i++)
        {
            if (arrays[i].gameObject.activeInHierarchy && i < arrays.Length - 1)
            {
              
                return;
            }
            else if(arrays.Length == 0)
            {
                Door.SetActive(false);
            }
        }
    }
}

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
           
        }
    }
    private void Update()
    {
        if (AllDestroyed(arrays))
        {

            Door.SetActive(false);
            return;
        }
    }
    bool AllDestroyed(GameObject[] arrays)
    {
        if (arrays == null || arrays.Length == 0)
            return true; // Empty array counts as "all destroyed"

        foreach (var obj in arrays)
        {
            if (obj != null) // Unity null check works for destroyed objects
                return false;
        }
        return true;
    }

}

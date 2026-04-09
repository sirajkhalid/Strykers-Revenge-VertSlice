using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MonsterTrigger1 : MonoBehaviour
{
    public GameObject Door;
    public GameObject Door1;
    public GameObject[] arrays;

    public void OnTriggerExit2D(Collider2D other)
    {
        Door.SetActive(true);
        Door1.SetActive(true);
        if(AllDestroyed(arrays))
        {
           
            Door.SetActive(false);
            Door1.SetActive(false);
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

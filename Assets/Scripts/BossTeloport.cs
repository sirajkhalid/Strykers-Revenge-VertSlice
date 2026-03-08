using UnityEngine;

public class BossTeloport : MonoBehaviour
{
    public GameObject Door;
    public GameObject[] arrays;

    
    
    private void Update()
    {
        for (int i = 0; i < arrays.Length; i++)
        {
            if (arrays[i].gameObject.activeInHierarchy && i < arrays.Length - 1)
            {

                return;
            }

        }
        if (AllDestroyed(arrays))
        {

            Door.SetActive(true);
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

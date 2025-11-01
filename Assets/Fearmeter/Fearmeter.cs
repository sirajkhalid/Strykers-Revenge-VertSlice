using UnityEngine;
using UnityEngine.UI;

public class Fearmeter : MonoBehaviour
{

    public Slider Fillimage;
    public float currenthealth;


    public void SetMaxHealth(int health)
    {
        Fillimage.maxValue = health;
        Fillimage.value = health;
        
}

    public void SetHealth(int health)
    {
        Fillimage.value = health;
    }
}

using UnityEngine;
using UnityEngine.UI;

public class PlayerProbe : MonoBehaviour
{
    public int maxHealth = 0;
    public int currentHealth;

    public Fearmeter fearmeter;
   

    void Start()
    {
        currentHealth = maxHealth;
        fearmeter.SetMaxHealth(maxHealth); // this is just a way to work around the actual condition and getting the bar to function probably.

    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {

            TakeDamage(10); // this will be the actual effect.
           
            
        }

    }

    void TakeDamage(int fear)
    {
        currentHealth -= fear;
        fearmeter.SetHealth(currentHealth);
    }
   
}

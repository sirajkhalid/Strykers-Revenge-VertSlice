using TMPro;
using UnityEngine;

public class ImagePlayer : MonoBehaviour
{

    public ClassSelection classDB;

    
    public SpriteRenderer artworkSprite;
    public GameObject CharacterScript;

    private int selectedOption = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!PlayerPrefs.HasKey("selectedOption"))
        {
            selectedOption = 0;

        }
        else
        {
            Load();
        }
        UpdatedCharacter(selectedOption);
    }
    private void UpdatedCharacter(int selectedOption) // how to select said option.
    {
        Character character = classDB.GetCharacter(selectedOption);
        artworkSprite.sprite = character.characterObject;
        

    }

    private void Load()
    {
        selectedOption = PlayerPrefs.GetInt("selectedOption");
    }


}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterManager : MonoBehaviour
{
    public ClassSelection classDB;

    public TMP_Text nameText;
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
    public void NextOption() //Button that pushes the menu forward
    {
        selectedOption++;

        if (selectedOption >= classDB.CharacterCount)
        {
            selectedOption = 0;
        }

        UpdatedCharacter(selectedOption);
        Save();
    }

    public void BackOption() // pushes the menu backward
    {
        selectedOption--;

        if(selectedOption < 0)
        {
            selectedOption = classDB.CharacterCount - 1;
        }

        UpdatedCharacter(selectedOption);
        Save();
    }
    private void UpdatedCharacter(int selectedOption) // how to select said option.
    {
        Character character = classDB.GetCharacter(selectedOption);
        artworkSprite.sprite = character.characterObject;
        nameText.text = character.characterName;

    }

    private void Load()
    {
        selectedOption = PlayerPrefs.GetInt("selectedOption");
    }

    private void Save()
    {
        PlayerPrefs.SetInt("selectedOption", selectedOption);
    }
    public void ChangeScene(int sceneID)
    {
        SceneManager.LoadScene(sceneID);
    }
  
}

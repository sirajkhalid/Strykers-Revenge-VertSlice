using PixelCrushers;
using System.Collections;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Reset : MonoBehaviour
{
    [SerializeField] private float ResetButtonShowDelay = 5f;
    [SerializeField] private GameObject ResetButton;
    [SerializeField] private bool UseLoadingIntermediaryScene = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private IEnumerator Start()
    {
        ResetButton.gameObject.SetActive(false);
        yield return new WaitForSeconds(ResetButtonShowDelay);
        ResetButton.gameObject.SetActive(true);
    }

    public void ResetLevel()
    {
        SceneManager.LoadSceneAsync(UseLoadingIntermediaryScene ? "Castle1" : "Elven Tower");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

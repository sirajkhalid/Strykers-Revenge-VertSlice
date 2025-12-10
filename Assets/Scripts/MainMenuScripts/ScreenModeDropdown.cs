using UnityEngine;
using TMPro;

public class ScreenModeDropdown : MonoBehaviour
{
    [SerializeField] TMP_Dropdown dropdown;
    [SerializeField] int windowedWidth = 1600;
    [SerializeField] int windowedHeight = 900;

    // 0 = Windowed, 1 = Borderless, 2 = Exclusive
    const string PP_MODE = "video_screenmode";

    void Awake()
    {
        if (!dropdown) dropdown = GetComponent<TMP_Dropdown>();

        // Ensure options exist
        if (dropdown.options == null || dropdown.options.Count == 0)
        {
            dropdown.ClearOptions();
            dropdown.AddOptions(new System.Collections.Generic.List<string>
            { "Windowed", "Borderless", "Exclusive" });
        }

        dropdown.onValueChanged.AddListener(OnDropdownChanged);

        int saved = Mathf.Clamp(PlayerPrefs.GetInt(PP_MODE, 1), 0, 2);
        dropdown.SetValueWithoutNotify(saved);
        ApplyMode(saved);
    }

    public void OnDropdownChanged(int index)
    {
        ApplyMode(index);
        PlayerPrefs.SetInt(PP_MODE, index);
    }

    void ApplyMode(int index)
    {
        switch (index)
        {
            case 0: // Windowed
                Screen.fullScreenMode = FullScreenMode.Windowed;
                Screen.SetResolution(windowedWidth, windowedHeight, FullScreenMode.Windowed);
                break;

            case 1: // Borderless (fullscreen window)
                var res1 = Screen.currentResolution;
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                Screen.SetResolution(res1.width, res1.height, FullScreenMode.FullScreenWindow);
                break;

            case 2: // Exclusive Fullscreen
                var res2 = Screen.currentResolution;
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                Screen.SetResolution(res2.width, res2.height, FullScreenMode.ExclusiveFullScreen);
                break;
        }
    }
}

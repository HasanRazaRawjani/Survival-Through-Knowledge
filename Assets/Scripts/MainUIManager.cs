using UnityEngine;
using UnityEngine.SceneManagement;

public class MainUIManager : MonoBehaviour
{
    [Header("Menu Panels")]
    [Tooltip("Drag your StartScene parent panel object here")]
    public GameObject startScenePanel;

    [Tooltip("Drag your Controls parent panel object here")]
    public GameObject controlsPanel;

    void Start()
    {
        if (startScenePanel != null)
        {
            startScenePanel.SetActive(true);
        }

        if (controlsPanel != null)
        {
            controlsPanel.SetActive(false);
        }
    }

    public void OpenHelpMenu()
    {
        if (startScenePanel != null) startScenePanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(true);
    }

    public void CloseHelpMenu()
    {
        if (controlsPanel != null) controlsPanel.SetActive(false);
        if (startScenePanel != null) startScenePanel.SetActive(true);
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("Solar System");
    }
}
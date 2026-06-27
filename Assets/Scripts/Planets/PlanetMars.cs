using UnityEngine;
using UnityEngine.SceneManagement;

public class PlanetMars : MonoBehaviour
{
    [Header("Planet Information")]
    public string planetName;
    [TextArea] public string planetDescription;

    private void OnMouseDown()
    {
        Debug.Log($"You clicked on {planetName}! Description: {planetDescription}");


        TriggerPlanetAction();
    }

    void TriggerPlanetAction()
    {
        if (!string.IsNullOrEmpty("Mars Terrain"))
        {
            SceneManager.LoadScene("Mars Terrain");
        }
        else
        {
            Debug.LogWarning($"No scene name assigned to the 'sceneToLoad' field on {gameObject.name}!");
        }
    }
}
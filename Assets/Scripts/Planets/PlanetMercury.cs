using UnityEngine;
using UnityEngine.SceneManagement;

public class PlanetMercury : MonoBehaviour
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
        if (!string.IsNullOrEmpty("Mercury Terrain"))
        {
            SceneManager.LoadScene("Mercury Terrain");
        }
        else
        {
            Debug.LogWarning($"No scene name assigned to the 'sceneToLoad' field on {gameObject.name}!");
        }
    }
}
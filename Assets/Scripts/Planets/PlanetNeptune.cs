using UnityEngine;
using UnityEngine.SceneManagement;

public class PlanetNeptune : MonoBehaviour
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
        if (!string.IsNullOrEmpty("Neptune Terrain"))
        {
            SceneManager.LoadScene("Neptune Terraind");
        }
        else
        {
            Debug.LogWarning($"No scene name assigned to the 'sceneToLoad' field on {gameObject.name}!");
        }
    }
}
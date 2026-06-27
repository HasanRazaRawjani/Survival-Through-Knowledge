using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }

    [System.Serializable]
    public class TriviaQuestion
    {
        public string question;
        public bool answer;
    }

    [System.Serializable]
    private class OllamaRequestData
    {
        public string model;
        public string prompt;
        public bool stream;
        public string format;
    }

    [System.Serializable]
    private class OllamaResponseData
    {
        public string response;
    }

    [Header("Endgame Scenes")]
    public GameObject WinScene;
    public GameObject DeathScene;

    [Header("Endgame Audio Settings")]
    [Tooltip("Drag your VictorySound AudioSource game object here")]
    public AudioSource victoryAudioSource;
    [Tooltip("Drag your LoseSound AudioSource game object here")]
    public AudioSource loseAudioSource;

    [Header("Main HUD Panels to Hide")]
    [Tooltip("Drag the Kill_Slider parent object here")]
    public GameObject killSliderPanel;
    [Tooltip("Drag the ProgressSlider parent object here")]
    public GameObject progressSliderPanel;
    [Tooltip("Drag the Crosshair game object here")]
    public GameObject crosshairObject;

    [Header("AI Question Screen UI")]
    public GameObject QuestionScreen;
    public Slider killsSlider;
    public TextMeshProUGUI QuestionText;
    public Slider progressSlider;

    [Header("Player Settings")]
    [Tooltip("Drag your Player object or FPS Controller script component here")]
    public MonoBehaviour fpsControllerScript;

    [Header("Lerp Settings")]
    public float lerpSpeed = 5f;

    [Header("Local AI Settings")]
    public string localModelName = "qwen2.5:1.5b";
    private string localOllamaUrl = "http://localhost:11434/api/generate";

    private int kills = 0;
    private int currentPoints = 0;
    private const int maxPoints = 10;
    private bool currentCorrectAnswer;
    private bool awaitingAIResponse = false;
    private bool gameHasEnded = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        killsSlider.value = 0f;
        QuestionScreen.SetActive(false);

        if (progressSlider != null)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = maxPoints;
            progressSlider.value = 0f;
        }

        if (WinScene != null) WinScene.SetActive(false);
        if (DeathScene != null) DeathScene.SetActive(false);
    }

    void Update()
    {
        if (gameHasEnded) return;

        float targetKillsValue = (float)kills / 3f;
        killsSlider.value = Mathf.Lerp(killsSlider.value, targetKillsValue, Time.deltaTime * lerpSpeed);

        if (progressSlider != null)
        {
            progressSlider.value = Mathf.Lerp(progressSlider.value, currentPoints, Time.deltaTime * lerpSpeed);
        }

        if (kills >= 3 && killsSlider.value >= 0.99f && !awaitingAIResponse)
        {
            killsSlider.value = 1f;
            GiveQuestion();
            kills = 0;
        }
    }

    public void AddKill()
    {
        if (gameHasEnded) return;
        kills++;
    }

    void GiveQuestion()
    {
        Time.timeScale = 0;
        AudioListener.pause = true;

        if (fpsControllerScript != null)
        {
            fpsControllerScript.enabled = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        QuestionScreen.SetActive(true);

        string currentPlanet = SceneManager.GetActiveScene().name.Replace(" Terrain", "");
        StartCoroutine(FetchAIQuestion(currentPlanet));
    }

    IEnumerator FetchAIQuestion(string planet)
    {
        awaitingAIResponse = true;
        QuestionText.text = "Generating question via local AI...";

        string systemPrompt = $"You are a trivia generator. Generate one unique, accurate True or False factual statement about the planet {planet}. " +
                             "CRITICAL: Do NOT write a question. Do NOT use a question mark. It must be a direct statement that can be answered with True or False. " +
                             "Example of an acceptable statement: \"Uranus is the 8th planet from the Sun.\" " +
                             "You must respond ONLY using this raw JSON structure: " +
                             "{\"question\": \"Your factual statement here\", \"answer\": true} or {\"question\": \"Your factual statement here\", \"answer\": false}. " +
                             "Do not include any introductory remarks, markdown formatting, or trailing text.";

        OllamaRequestData requestBody = new OllamaRequestData
        {
            model = localModelName,
            prompt = systemPrompt,
            stream = false,
            format = "json"
        };

        string jsonPayload = JsonUtility.ToJson(requestBody);

        using (UnityWebRequest webRequest = new UnityWebRequest(localOllamaUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                OllamaResponseData outerContent = JsonUtility.FromJson<OllamaResponseData>(webRequest.downloadHandler.text);
                TriviaQuestion generatedQuestion = JsonUtility.FromJson<TriviaQuestion>(outerContent.response);

                QuestionText.text = generatedQuestion.question;
                currentCorrectAnswer = generatedQuestion.answer;
            }
            else
            {
                QuestionText.text = "Failed to connect to local AI engine.";
                currentCorrectAnswer = true;
            }
        }
        awaitingAIResponse = false;
    }

    private void ResumeGame()
    {
        if (gameHasEnded) return;

        Time.timeScale = 1;
        AudioListener.pause = false;

        if (fpsControllerScript != null)
        {
            fpsControllerScript.enabled = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        QuestionScreen.SetActive(false);
    }

    public void TrueClicked()
    {
        EvaluateAnswer(true);
    }

    public void FalseClicked()
    {
        EvaluateAnswer(false);
    }

    private void EvaluateAnswer(bool playerChoice)
    {
        if (playerChoice == currentCorrectAnswer)
        {
            currentPoints += 2;
        }
        else
        {
            currentPoints -= 1;
        }

        currentPoints = Mathf.Clamp(currentPoints, 0, maxPoints);

        if (currentPoints >= maxPoints)
        {
            TriggerWin();
        }
        else
        {
            ResumeGame();
        }
    }

    private void HideAllOtherUI()
    {
        if (killSliderPanel != null) killSliderPanel.SetActive(false);
        if (progressSliderPanel != null) progressSliderPanel.SetActive(false);
        if (crosshairObject != null) crosshairObject.SetActive(false);

        Canvas myCanvas = GetComponent<Canvas>();
        if (myCanvas == null)
        {
            myCanvas = GetComponentInParent<Canvas>();
        }

        Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (Canvas canvas in allCanvases)
        {
            if (canvas != myCanvas)
            {
                canvas.gameObject.SetActive(false);
            }
        }
    }

    private void TriggerWin()
    {
        gameHasEnded = true;
        QuestionScreen.SetActive(false);
        HideAllOtherUI();

        if (victoryAudioSource != null)
        {
            victoryAudioSource.ignoreListenerPause = true;
            victoryAudioSource.Play();
        }

        Time.timeScale = 0;
        AudioListener.pause = true;

        if (fpsControllerScript != null)
        {
            fpsControllerScript.enabled = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (WinScene != null)
        {
            WinScene.SetActive(true);
        }
    }

    public void TriggerPlayerDeath()
    {
        if (gameHasEnded) return;
        gameHasEnded = true;
        QuestionScreen.SetActive(false);
        HideAllOtherUI();

        if (loseAudioSource != null)
        {
            loseAudioSource.ignoreListenerPause = true;
            loseAudioSource.Play();
        }

        Time.timeScale = 0;
        AudioListener.pause = true;

        if (fpsControllerScript != null)
        {
            fpsControllerScript.enabled = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (DeathScene != null)
        {
            DeathScene.SetActive(true);
        }
    }

    public void LoadSelectScene()
    {
        Time.timeScale = 1;
        AudioListener.pause = false;
        SceneManager.LoadScene("SelectScene");
    }
}
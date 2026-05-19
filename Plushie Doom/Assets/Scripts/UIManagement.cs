using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManagement : MonoBehaviour
{
    public static UIManagement Instance;

    [Header("Global Entities")]
    public GameObject playerObject;
    public Camera globalCamera;
    public GameObject mainCanvas;

    [Header("Fader Settings")]
    public CanvasGroup faderGroup;
    public float fadeDuration = 0.5f;

    private string targetSpawnerId;
    private bool isTransitioning = false;
    public bool IsTransitioning => isTransitioning;

    private void Awake()
{
    if (Instance == null)
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Ініціалізація посилань
        if (globalCamera == null) globalCamera = GetComponentInChildren<Camera>();
        if (playerObject == null) playerObject = GameObject.FindGameObjectWithTag("Player");
    }
    else
    {
        // ВАЖЛИВО: видаляємо дублікат негайно, щоб його UI не встиг "провітритися"
        DestroyImmediate(gameObject);
    }
}

    private void Start()
    {
        if (faderGroup != null)
        {
            faderGroup.alpha = 1f;
            StartCoroutine(Fade(0f));
        }
    }

    public void TransitionToScene(string sceneName, string spawnerId)
    {
        if (isTransitioning) return;
        isTransitioning = true;
        targetSpawnerId = spawnerId;
        StartCoroutine(TransitionCoroutine(sceneName));
    }

    private IEnumerator TransitionCoroutine(string sceneName)
    {
        yield return StartCoroutine(Fade(1f));

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false; 

        yield return new WaitForSeconds(1f); // Твоя гарантована пауза в темряві

        asyncLoad.allowSceneActivation = true;
        while (!asyncLoad.isDone) yield return null;

        // Після завантаження висвітлюємо екран
        yield return StartCoroutine(Fade(0f));
        isTransitioning = false;
    }

    private void MovePlayerToTargetSpawner()
    {
        PlayerSpawner[] spawners = FindObjectsByType<PlayerSpawner>(FindObjectsSortMode.None);
        bool found = false;

        foreach (var spawner in spawners)
        {
            if (spawner.spawnerId == targetSpawnerId)
            {
                playerObject.transform.position = spawner.transform.position;
                Debug.Log($"[UIManagement] Гравець спавниться на точці: {targetSpawnerId}");
                found = true;
                break;
            }
        }

        if (!found)
        {
            Debug.LogWarning($"[UIManagement] СПАВНЕР '{targetSpawnerId}' НЕ ЗНАЙДЕНО! Гравець залишився на старій позиції.");
        }
    }

    private void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    private void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CleanUpDuplicateCameras();

        if (isTransitioning && playerObject != null)
        {
            MovePlayerToTargetSpawner();
        }
        
        // Тут можна оновити ціль камери, якщо ти використовуєш Cinemachine або скрипт переслідування
        UpdateCameraFollow();
    }

    private void UpdateCameraFollow()
    {
        // Якщо у камери є скрипт переслідування, передаємо йому playerObject.transform
        // Наприклад: globalCamera.GetComponent<CameraFollow>().target = playerObject.transform;
    }

    private void CleanUpDuplicateCameras()
    {
        Camera[] cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
        foreach (Camera cam in cameras)
        {
            if (cam != globalCamera) Destroy(cam.gameObject);
        }
    }

    private IEnumerator Fade(float targetAlpha)
    {
        if (faderGroup == null) 
        {
            Debug.LogWarning("FaderGroup is missing!");
            yield break;
        }
        float startAlpha = faderGroup.alpha;
        float time = 0;
        while (time < fadeDuration)
        {
            if (faderGroup == null) yield break;
            time += Time.deltaTime;
            faderGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            yield return null;
        }
        faderGroup.alpha = targetAlpha;
    }
}
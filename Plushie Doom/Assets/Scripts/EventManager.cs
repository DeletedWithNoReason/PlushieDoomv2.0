using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EventManager : MonoBehaviour
{
public static EventManager Instance;

    [Header("Settings")]
    public bool IsEventRunning { get; private set; }
    private Coroutine currentSequence;

    // Глобальні дані напарника
    public static string ActiveFollowerPrefabName = "";
    public static bool IsFollowingActive = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DestroyImmediate(gameObject);
            return;
        }
    }

    void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckAndSpawnFollower();
    }

    public void CheckAndSpawnFollower()
    {
        if (IsFollowingActive && !string.IsNullOrEmpty(ActiveFollowerPrefabName))
        {
            bool found = false;
            foreach (var npc in FindObjectsByType<NPCFollower>(FindObjectsSortMode.None))
            {
                if (npc.gameObject.name.StartsWith(ActiveFollowerPrefabName))
                {
                    found = true;
                    npc.SetFollowing(true);
                    break;
                }
            }

            if (!found) SpawnFollowerFromResources();
        }
    }

    private void SpawnFollowerFromResources()
    {
        GameObject prefab = Resources.Load<GameObject>(ActiveFollowerPrefabName);
        if (prefab != null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Vector3 spawnPos = new Vector3(player.transform.position.x + 1f, player.transform.position.y, player.transform.position.z);
                GameObject npc = Instantiate(prefab, spawnPos, Quaternion.identity);
                npc.name = ActiveFollowerPrefabName;
                npc.GetComponent<NPCFollower>()?.SetFollowing(true);
            }
        }
    }

    // Стандартний запуск події
    public void RunEvent(GameEvent gameEvent)
    {
        if (gameEvent == null || IsEventRunning) return;
        currentSequence = StartCoroutine(ExecuteSequence(gameEvent));
    }

    // МЕТОД ДЛЯ CHOICE MANAGER: перериває поточну чергу і запускає нову
    public void ForceRunEvent(GameEvent gameEvent)
    {
        if (currentSequence != null) StopCoroutine(currentSequence);
        IsEventRunning = false; 
        RunEvent(gameEvent);
    }

    private IEnumerator ExecuteSequence(GameEvent gameEvent)
    {
        IsEventRunning = true;
        foreach (var action in gameEvent.actions)
        {
            switch (action.type)
            {
                case EventAction.ActionType.Dialogue:
                    DialogueManager.Instance?.StartDialogue(action.dialogueData, action.abruptStart, action.abruptEnd);
                    while (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueBusy()) yield return null;
                    break;

                case EventAction.ActionType.CharFollow:
                    ActiveFollowerPrefabName = action.npcName;
                    IsFollowingActive = action.startFollowing;

                    bool foundOnScene = false;
                    foreach (var npc in FindObjectsByType<NPCFollower>(FindObjectsSortMode.None))
                    {
                        if (npc.gameObject.name.StartsWith(action.npcName))
                        {
                            npc.SetFollowing(action.startFollowing);
                            foundOnScene = true;
                        }
                    }

                    if (IsFollowingActive && !foundOnScene) CheckAndSpawnFollower();
                    break;

                case EventAction.ActionType.ToggleObject:
                    GameObject target = FindInActiveObjectByName(action.targetObjectName);
                    if (target != null) target.SetActive(action.toggleState);
                    break;

                case EventAction.ActionType.PlaySound:
                    AudioManager.Instance?.PlayBackgroundMusic(action.audioToPlay);
                    break;

                case EventAction.ActionType.Wait:
                    yield return new WaitForSeconds(action.waitTime);
                    break;

                case EventAction.ActionType.ChangeScene:
                    UIManagement.Instance?.TransitionToScene(action.sceneName, action.targetSpawnerId);
                    break;

                case EventAction.ActionType.ShowChoice:
                    ChoiceManager.Instance?.ShowChoices(action.choiceData);
                    // Очікуємо, поки ChoiceManager не вибере варіант (який викличе ForceRunEvent)
                    while (ChoiceManager.Instance != null && ChoiceManager.Instance.IsBusy()) yield return null;
                    break;
            }
        }
        IsEventRunning = false;
        currentSequence = null;
    }

    private GameObject FindInActiveObjectByName(string name)
    {
        Transform[] allTransforms = Resources.FindObjectsOfTypeAll<Transform>();
        foreach (Transform t in allTransforms)
        {
            if (t.name == name && t.gameObject.scene.name != null) return t.gameObject;
        }
        return null;
    }
}
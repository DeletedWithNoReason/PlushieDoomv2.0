using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class AutoStartScene
{

    private const string StartupScene = "Assets/Scenes/MainMenu.unity";


    private const string PreviousSceneKey = "PreviousScene";

    static AutoStartScene()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {

        if (state == PlayModeStateChange.ExitingEditMode)
        {
            string currentScene = SceneManager.GetActiveScene().path;

            // Зберігаємо поточну сцену
            EditorPrefs.SetString(PreviousSceneKey, currentScene);

            // Якщо вже не стартова сцена — відкриваємо стартову
            if (currentScene != StartupScene)
            {
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                EditorSceneManager.OpenScene(StartupScene);
            }
        }

        // Після завершення Play Mode
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            string previousScene = EditorPrefs.GetString(PreviousSceneKey);

            if (!string.IsNullOrEmpty(previousScene))
            {
                EditorSceneManager.OpenScene(previousScene);
            }
        }
    }
}
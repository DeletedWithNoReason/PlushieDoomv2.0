using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class DialogueJsonImporter : EditorWindow
{
    [MenuItem("Tools/Import Dialogue from JSON")]
    public static void ImportJson()
    {
        string path = EditorUtility.OpenFilePanel("Оберіть файл діалогу (JSON)", "", "json");
        if (string.IsNullOrEmpty(path)) return;

        string jsonContent = File.ReadAllText(path);
        JsonDialogueData rawData = JsonUtility.FromJson<JsonDialogueData>(jsonContent);

        if (rawData == null || rawData.lines == null)
        {
            Debug.LogError("Помилка: Не вдалося прочитати структуру JSON!");
            return;
        }

        // Створюємо екземпляр ScriptableObject
        DialogueData asset = ScriptableObject.CreateInstance<DialogueData>();

        // Завантажуємо стартові спрайти
        asset.startLeft = Resources.Load<Sprite>(rawData.s_l);
        asset.startRight = Resources.Load<Sprite>(rawData.s_r);

        // Конвертуємо рядки
        List<DialogueLine> assetLines = new List<DialogueLine>();
        foreach (var rawLine in rawData.lines)
        {
            DialogueLine line = new DialogueLine
            {
                characterName = rawLine.c_n,
                text = rawLine.txt,
                isLeftCharacter = rawLine.is_l,
                customSpeed = rawLine.spd,
                muteSound = rawLine.m_s,
                // Конвертуємо Hex у Color
                textColor = HexToColor(rawLine.l_c),
                // Завантажуємо спрайти для конкретного рядка
                leftSprite = Resources.Load<Sprite>(rawLine.l_s),
                rightSprite = Resources.Load<Sprite>(rawLine.r_s)
            };
            assetLines.Add(line);
        }

        asset.lines = assetLines.ToArray();

        // Зберігаємо ассет
        string fileName = Path.GetFileNameWithoutExtension(path);
        AssetDatabase.CreateAsset(asset, $"Assets/D_CDataFiles/Events/Act1/{fileName}_Data.asset");
        AssetDatabase.SaveAssets();
        
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;

        Debug.Log($"<color=green>Успіх!</color> Діалог '{fileName}' створено в папці Assets.");
    }

    private static Color HexToColor(string hex)
    {
        if (string.IsNullOrEmpty(hex)) return Color.white;
        if (ColorUtility.TryParseHtmlString(hex, out Color color)) return color;
        return Color.white;
    }

    // Класи-обгортки для десеріалізації JSON (короткі назви)
    [System.Serializable]
    public class JsonDialogueData
    {
        public string s_l; // startLeft
        public string s_r; // startRight
        public List<JsonDialogueLine> lines;
    }

    [System.Serializable]
    public class JsonDialogueLine
    {
        public string c_n;  // characterName
        public string txt;  // text
        public string l_c;  // textColor (hex)
        public bool is_l;   // isLeftCharacter
        public string l_s;  // leftSprite
        public string r_s;  // rightSprite
        public float spd;   // customSpeed
        public bool m_s;    // muteSound
    }
}
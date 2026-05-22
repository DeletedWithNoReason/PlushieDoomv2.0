using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class DialogueJsonExporter : EditorWindow
{
    [MenuItem("Tools/Export Dialogue to JSON")]
    public static void ExportToJson()
    {
        // 1. Обираємо об'єкт ScriptableObject у проекті
        DialogueData selectedData = Selection.activeObject as DialogueData;

        if (selectedData == null)
        {
            EditorUtility.DisplayDialog("Помилка", "Будь ласка, оберіть файл DialogueData (ScriptableObject) у вікні Project!", "ОК");
            return;
        }

        // 2. Обираємо шлях для збереження
        string path = EditorUtility.SaveFilePanel("Зберегти діалог як JSON", "", selectedData.name, "json");
        if (string.IsNullOrEmpty(path)) return;

        // 3. Конвертуємо дані у проміжний клас для JSON
        JsonDialogueData rawData = new JsonDialogueData
        {
            s_l = selectedData.startLeft != null ? selectedData.startLeft.name : "",
            s_r = selectedData.startRight != null ? selectedData.startRight.name : "",
            lines = new List<JsonDialogueLine>()
        };

        foreach (var line in selectedData.lines)
        {
            JsonDialogueLine jsonLine = new JsonDialogueLine
            {
                c_n = line.characterName,
                txt = line.text,
                is_l = line.isLeftCharacter,
                spd = line.customSpeed,
                m_s = line.muteSound,
                l_c = ColorToHex(line.textColor),
                l_s = line.leftSprite != null ? line.leftSprite.name : "",
                r_s = line.rightSprite != null ? line.rightSprite.name : ""
            };
            rawData.lines.Add(jsonLine);
        }

        // 4. Серіалізація в рядок
        string jsonContent = JsonUtility.ToJson(rawData, true); // true для гарного форматування (Pretty Print)

        // 5. Запис у файл
        File.WriteAllText(path, jsonContent);
        AssetDatabase.Refresh();

        Debug.Log($"<color=cyan>Успіх!</color> Діалог '{selectedData.name}' успішно експортовано у JSON: {path}");
    }

    private static string ColorToHex(Color color)
    {
        // Повертає формат #RRGGBBAA або #RRGGBB
        return "#" + ColorUtility.ToHtmlStringRGBA(color);
    }

    // Класи-обгортки (точно такі ж, як в імпортері для сумісності)
    [System.Serializable]
    public class JsonDialogueData
    {
        public string s_l; 
        public string s_r; 
        public List<JsonDialogueLine> lines;
    }

    [System.Serializable]
    public class JsonDialogueLine
    {
        public string c_n;  
        public string txt;  
        public string l_c;  
        public bool is_l;   
        public string l_s;  
        public string r_s;  
        public float spd;   
        public bool m_s;    
    }
}
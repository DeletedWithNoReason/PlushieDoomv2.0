using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    public string characterName; 
    [TextArea(3, 10)]
    public string text; 
    public Color textColor = Color.white; 
    public bool isLeftCharacter;

    [Header("Sprites for this line")]
    public Sprite leftSprite;
    public Sprite rightSprite;
    
    public float customSpeed = 0.05f;
    public bool muteSound = false; 
}

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue System/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    public Sprite startLeft;
    public Sprite startRight;

    public DialogueLine[] lines;
}
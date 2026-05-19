using UnityEngine;

[System.Serializable]
public class CutsceneStep
{
    public string characterName;
    [TextArea(3, 10)]
    public string text;
    public Sprite backgroundSprite;
    public float typingSpeed = 0.05f;

    [Header("Audio")]
    public AudioClip stepSound;
    public bool muteSound = false; 
    public bool muteTyping = false; 

    [Header("UI Control")]
    public bool togglePanel = true;
}

[CreateAssetMenu(fileName = "NewCutscene", menuName = "Cutscene/Cutscene Data")]
public class CutsceneData : ScriptableObject
{
    public CutsceneStep[] steps;
}
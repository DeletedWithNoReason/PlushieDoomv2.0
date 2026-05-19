using UnityEngine;

[System.Serializable]
public struct ChoiceOption
{
    public string optionText; // Текст на кнопці
    public GameEvent resultingEvent; // Що станеться після вибору
}

[CreateAssetMenu(fileName = "NewChoice", menuName = "Events/Choice Data")]
public class ChoiceData : ScriptableObject
{
    [TextArea(3, 5)]
    public string questionText;
    public ChoiceOption[] options;
}
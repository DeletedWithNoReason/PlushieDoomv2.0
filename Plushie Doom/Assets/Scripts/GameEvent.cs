using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct EventAction
{
    public enum ActionType { Dialogue, Cutscene, ToggleObject, Wait, ChangeScene, ShowChoice, CharFollow, PlaySound }

    public ActionType type;

    [Header("Data References")]
    public DialogueData dialogueData;
    public CutsceneData cutsceneData;
    public ChoiceData choiceData;

    [Header("Object Control")]
    public string targetObjectName;
    public bool toggleState;

    [Header("NPC Control")]
    public string npcName; 
    public bool startFollowing; 

    [Header("Settings")]
    public float waitTime;
    public string sceneName;
    public string targetSpawnerId;

    [Header("Abrupt Control")]
    public bool abruptStart;
    public bool abruptEnd;
    
    [Header("Play Sound")]
    public AudioClip audioToPlay;
}

[CreateAssetMenu(fileName = "NewEvent", menuName = "Events/Game Event")]
public class GameEvent : ScriptableObject
{
    public List<EventAction> actions;
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ChoiceManager : MonoBehaviour
{
    public static ChoiceManager Instance;
    public CanvasGroup panelGroup;
    public TextMeshProUGUI questionText;
    public Button[] choiceButtons; // Признач сюди кнопки в інспекторі

    private bool isBusy = false;

    void Awake() 
    {
        foreach (var button in choiceButtons) {
        // Додаємо просту перевірку наведення (Hover)
        var trigger = button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        var entry = new UnityEngine.EventSystems.EventTrigger.Entry();
        entry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
        entry.callback.AddListener((data) => { Debug.Log($"[Choice] Миша над кнопкою: {button.name}"); });
        trigger.triggers.Add(entry);
        }
        if (Instance == null) 
        {
            Instance = this;
            // Тут НЕ треба DontDestroyOnLoad, бо він уже є в UIManagement на цьому ж об'єкті
        }
        else 
        {
            // Це критично важливо, щоб TMP не встиг видати помилку
            DestroyImmediate(gameObject); 
            return;
        }
        
        panelGroup.alpha = 0;
        panelGroup.blocksRaycasts = false;
    }

    public bool IsBusy() => isBusy;

    public void ShowChoices(ChoiceData data)
    {
        isBusy = true;
        panelGroup.alpha = 1;
        panelGroup.blocksRaycasts = true;
        questionText.text = data.questionText;

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (i < data.options.Length)
            {
                choiceButtons[i].gameObject.SetActive(true);
                choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = data.options[i].optionText;
                
                // Запам'ятовуємо локальну змінну для замикання
                int index = i; 
                choiceButtons[i].onClick.RemoveAllListeners();
                choiceButtons[i].onClick.AddListener(() => SelectOption(data.options[index].resultingEvent));
            }
            else
            {
                choiceButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void SelectOption(GameEvent nextEvent)
    {
        Debug.Log($"[Choice] Вибрано варіант! Наступна подія: {(nextEvent != null ? nextEvent.name : "null")}");
        panelGroup.alpha = 0;
        panelGroup.blocksRaycasts = false;
        isBusy = false;
        
        // Запускаємо наступний ланцюжок подій
        if (nextEvent != null) EventManager.Instance.ForceRunEvent(nextEvent);
    }
}
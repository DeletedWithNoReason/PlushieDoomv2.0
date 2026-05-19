using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager Instance;

    [Header("UI Components")]
    public TextMeshProUGUI textComponent;
    public TextMeshProUGUI nameComponent;
    public GameObject dialoguePanel;
    private CanvasGroup panelGroup;
    private CanvasGroup managerGroup;
    
    [Header("Visual Elements")]
    public Image faderImage;
    public Image backgroundA;
    public Image backgroundB;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip typeSound;

    private CutsceneData currentData;
    private int index;
    private bool isActive = false;
    private bool isAnimating = false;
    private bool currentAbruptEnd = false;

    private Coroutine typingCoroutine;
    private Coroutine bgCoroutine;
    private Coroutine panelCoroutine;

    void Awake()
    {
        if (Instance == null) Instance = this;
        panelGroup = dialoguePanel.GetComponent<CanvasGroup>();
        if (panelGroup == null) panelGroup = dialoguePanel.AddComponent<CanvasGroup>();
        managerGroup = GetComponent<CanvasGroup>();
        if (managerGroup == null) managerGroup = gameObject.AddComponent<CanvasGroup>();

        if (backgroundA != null) SetImageAlpha(backgroundA, 1f);
        if (backgroundB != null) SetImageAlpha(backgroundB, 0f);

        managerGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    public bool IsBusy() => isActive || isAnimating;

    public void StartCutscene(CutsceneData data, bool abruptStart = false, bool abruptEnd = false)
    {
        if (data == null || data.steps.Length == 0) return;

        currentData = data;
        index = 0;
        isActive = true;
        isAnimating = false;
        currentAbruptEnd = abruptEnd;

        gameObject.SetActive(true);
        managerGroup.alpha = 1f;

        if (audioSource != null) audioSource.volume = 1f;
        panelGroup.alpha = currentData.steps[0].togglePanel ? 1f : 0f;

        if (abruptStart)
        {
            if (faderImage != null) faderImage.color = new Color(0, 0, 0, 0);
            StartCoroutine(ExecuteSequenceAbrupt());
        }
        else
        {
            if (faderImage != null) { faderImage.gameObject.SetActive(true); faderImage.color = new Color(0, 0, 0, 1); }
            StartCoroutine(ExecuteSequenceFade());
        }
    }

    IEnumerator ExecuteSequenceFade()
    {
        if (currentData.steps[index].backgroundSprite != null) backgroundA.sprite = currentData.steps[index].backgroundSprite;
        yield return new WaitForSeconds(0.5f);
        if (faderImage != null) yield return StartCoroutine(FadeImage(faderImage, 0f, 1.5f));
        PlayStepAudio(index);
        typingCoroutine = StartCoroutine(TypeLine());
    }

    IEnumerator ExecuteSequenceAbrupt()
    {
        if (currentData.steps[index].backgroundSprite != null) backgroundA.sprite = currentData.steps[index].backgroundSprite;
        PlayStepAudio(index);
        typingCoroutine = StartCoroutine(TypeLine());
        yield break;
    }

    // Решта твоєї логіки (Update, NextStep, ABA Transition, TypeLine) залишається
    void Update()
    {
        if (!isActive || isAnimating) return;
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            var step = currentData.steps[index];
            if (!step.togglePanel) { NextStep(); return; }
            if (textComponent.text == step.text) NextStep();
            else SkipTyping(step.text);
        }
    }

    void NextStep()
    {
        if (index < currentData.steps.Length - 1)
        {
            index++;
            var step = currentData.steps[index];
            UpdatePanel(step.togglePanel);
            PlayStepAudio(index);
            if (step.backgroundSprite != null && step.backgroundSprite != backgroundA.sprite)
                UpdateBackground(step.backgroundSprite, 1.0f);
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            if (step.togglePanel) typingCoroutine = StartCoroutine(TypeLine());
            else textComponent.text = "";
        }
        else StartCoroutine(EndCutsceneRoutine());
    }

    IEnumerator EndCutsceneRoutine()
    {
        isActive = false;
        if (currentAbruptEnd)
        {
            managerGroup.alpha = 0f;
            gameObject.SetActive(false);
        }
        else
        {
            isAnimating = true; 
            float duration = 1.5f;
            float elapsed = 0;
            float startAlpha = managerGroup.alpha;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                managerGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / duration);
                if (audioSource != null) audioSource.volume = Mathf.Lerp(1f, 0f, elapsed / duration);
                yield return null;
            }
            managerGroup.alpha = 0f;
            isAnimating = false;
            gameObject.SetActive(false);
        }
    }

    // Допоміжні методи ABA, FadeImage, SetImageAlpha, TypeLine, SkipTyping, PlayStepAudio, UpdatePanel...
    // (Скопіюй їх зі свого старого CutsceneManager)
    public void UpdateBackground(Sprite nextSprite, float duration) { if (bgCoroutine != null) StopCoroutine(bgCoroutine); bgCoroutine = StartCoroutine(ABA_Transition(nextSprite, duration)); }
    IEnumerator ABA_Transition(Sprite nextSprite, float duration) { backgroundB.sprite = nextSprite; yield return StartCoroutine(FadeImage(backgroundB, 1f, duration)); backgroundA.sprite = nextSprite; SetImageAlpha(backgroundB, 0f); bgCoroutine = null; }
    IEnumerator FadeImage(Image img, float targetAlpha, float duration) { if (img == null) yield break; float startAlpha = img.color.a; float elapsed = 0; Color c = img.color; while (elapsed < duration) { elapsed += Time.deltaTime; c.a = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration); img.color = c; yield return null; } c.a = targetAlpha; img.color = c; if (targetAlpha <= 0) img.gameObject.SetActive(false); else img.gameObject.SetActive(true); }
    void SetImageAlpha(Image img, float alpha) { if (img == null) return; Color c = img.color; c.a = alpha; img.color = c; }
    IEnumerator TypeLine() { textComponent.text = string.Empty; nameComponent.text = currentData.steps[index].characterName; foreach (char c in currentData.steps[index].text.ToCharArray()) { textComponent.text += c; if (!currentData.steps[index].muteTyping && audioSource != null && typeSound != null && !char.IsWhiteSpace(c)) audioSource.PlayOneShot(typeSound); yield return new WaitForSeconds(currentData.steps[index].typingSpeed); } typingCoroutine = null; }
    void SkipTyping(string fullText) { if (typingCoroutine != null) StopCoroutine(typingCoroutine); textComponent.text = fullText; }
    void PlayStepAudio(int stepIndex) { var step = currentData.steps[stepIndex]; if (!step.muteSound && audioSource != null && step.stepSound != null) audioSource.PlayOneShot(step.stepSound); }
    void UpdatePanel(bool show) { if (panelCoroutine != null) StopCoroutine(panelCoroutine); panelCoroutine = StartCoroutine(FadeCanvasGroup(panelGroup, show ? 1f : 0f, 0.5f)); }
    IEnumerator FadeCanvasGroup(CanvasGroup cg, float targetAlpha, float duration) { float startAlpha = cg.alpha; float elapsed = 0; while (elapsed < duration) { elapsed += Time.deltaTime; cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration); yield return null; } cg.alpha = targetAlpha; }
}
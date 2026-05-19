using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour 
{
    public static DialogueManager Instance;

    [Header("UI Components")]
    public CanvasGroup dialogueUI; 
    public TextMeshProUGUI textComponent;
    public Image leftCharacterImage;
    public Image rightCharacterImage;

    [Header("Visual & Animation Settings")]
    public float fadeDuration = 0.5f;
    public float activeScale = 1.15f;
    public float animationDuration = 0.2f; 
    public Color inactiveColor = new Color(0.5f, 0.5f, 0.5f);

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip typeSound; 
    [Range(0, 1)] public float volume = 0.5f;

    private DialogueData currentData;
    private int index;
    private Coroutine typingCoroutine;
    private Coroutine animationCoroutine;
    private Coroutine fadeCoroutine;
    
    private bool isDialogueActive = false; 
    private bool isUIAnimating = false;
    private bool currentAbruptEnd = false;

    void Awake() 
    {
        if (Instance == null) Instance = this;
        
        // Початковий стан: невидимий і не заважає клікам
        if (dialogueUI != null)
        {
            dialogueUI.alpha = 0;
            dialogueUI.interactable = false;
            dialogueUI.blocksRaycasts = false;
        }
        isUIAnimating = false;
    }

    public bool IsDialogueBusy() => isDialogueActive || isUIAnimating;

    public void StartDialogue(DialogueData data, bool abruptStart = false, bool abruptEnd = false)
    {
        if (data == null || data.lines.Length == 0 || isUIAnimating) return;

        currentData = data;
        index = 0;
        isDialogueActive = true;
        currentAbruptEnd = abruptEnd;

        if (data.startLeft != null) leftCharacterImage.sprite = data.startLeft;
        if (data.startRight != null) rightCharacterImage.sprite = data.startRight;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

        if (abruptStart)
        {
            dialogueUI.alpha = 1f;
            dialogueUI.interactable = true;
            dialogueUI.blocksRaycasts = true;
            isUIAnimating = false;
        }
        else
        {
            fadeCoroutine = StartCoroutine(FadeUI(1f));
        }

        UpdateVisualsInstant(); 
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeLine());
    }

    void Update()
    {
        if (!isDialogueActive || isUIAnimating) return;
        
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            if (textComponent.text == currentData.lines[index].text) 
                NextLine();
            else 
                SkipTyping();
        }
    }

    public void CloseDialogue()
    {
        isDialogueActive = false;
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

        if (currentAbruptEnd)
        {
            dialogueUI.alpha = 0f;
            dialogueUI.interactable = false;
            dialogueUI.blocksRaycasts = false;
        }
        else
        {
            fadeCoroutine = StartCoroutine(FadeUI(0f));
        }
    }

    IEnumerator FadeUI(float targetAlpha)
    {
        isUIAnimating = true;
        float startAlpha = dialogueUI.alpha;
        float time = 0;

        // Якщо вмикаємо — одразу блокуємо кліки крізь панель
        if (targetAlpha > 0) dialogueUI.blocksRaycasts = true;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            dialogueUI.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            yield return null;
        }

        dialogueUI.alpha = targetAlpha;
        
        // Фінальні стани взаємодії
        dialogueUI.interactable = targetAlpha > 0;
        dialogueUI.blocksRaycasts = targetAlpha > 0;
        
        isUIAnimating = false;
    }

    IEnumerator TypeLine()
    {
        textComponent.text = string.Empty;
        Color cColor = currentData.lines[index].textColor;
        if(cColor.a == 0) cColor.a = 1f;
        textComponent.color = cColor;

        foreach (char c in currentData.lines[index].text.ToCharArray())
        {
            textComponent.text += c;
            if (!currentData.lines[index].muteSound && audioSource != null && typeSound != null && !char.IsWhiteSpace(c))
                audioSource.PlayOneShot(typeSound, volume);
            yield return new WaitForSeconds(currentData.lines[index].customSpeed);
        }
    }

    void NextLine()
    {
        if (index < currentData.lines.Length - 1)
        {
            index++;
            UpdateVisualsInstant(); 
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeLine());
            if (animationCoroutine != null) StopCoroutine(animationCoroutine);
            animationCoroutine = StartCoroutine(AnimateCharacters());
        }
        else CloseDialogue();
    }

    void SkipTyping()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        textComponent.text = currentData.lines[index].text;
        UpdateVisualsInstant(); 
    }

    void UpdateVisualsInstant()
    {
        var line = currentData.lines[index];
        if (line.leftSprite != null) leftCharacterImage.sprite = line.leftSprite;
        if (line.rightSprite != null) rightCharacterImage.sprite = line.rightSprite;
        
        bool isLeft = line.isLeftCharacter;

        // Лівий персонаж
        leftCharacterImage.transform.localScale = isLeft ? Vector3.one * activeScale : Vector3.one;

        // Правий персонаж (тепер без віддзеркалення, X = +rScale)
        float rScale = !isLeft ? activeScale : 1f;
        rightCharacterImage.transform.localScale = new Vector3(rScale, rScale, rScale);

        leftCharacterImage.color = isLeft ? Color.white : inactiveColor;
        rightCharacterImage.color = !isLeft ? Color.white : inactiveColor;
    }

    IEnumerator AnimateCharacters()
    {
        bool isLeft = currentData.lines[index].isLeftCharacter;

        Vector3 leftTargetScale = isLeft ? Vector3.one * activeScale : Vector3.one;
        float rTargetVal = !isLeft ? activeScale : 1f;
        // Тепер Scale X додатний
        Vector3 rightTargetScale = new Vector3(rTargetVal, rTargetVal, rTargetVal);

        Color leftTargetColor = isLeft ? Color.white : inactiveColor;
        Color rightTargetColor = !isLeft ? Color.white : inactiveColor;

        float elapsedTime = 0;
        Vector3 leftStartScale = leftCharacterImage.transform.localScale;
        Vector3 rightStartScale = rightCharacterImage.transform.localScale;
        Color leftStartColor = leftCharacterImage.color;
        Color rightStartColor = rightCharacterImage.color;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationDuration;

            leftCharacterImage.transform.localScale = Vector3.Lerp(leftStartScale, leftTargetScale, t);
            rightCharacterImage.transform.localScale = Vector3.Lerp(rightStartScale, rightTargetScale, t);
            
            leftCharacterImage.color = Color.Lerp(leftStartColor, leftTargetColor, t);
            rightCharacterImage.color = Color.Lerp(rightStartColor, rightTargetColor, t);
            yield return null;
        }
        
        leftCharacterImage.transform.localScale = leftTargetScale;
        rightCharacterImage.transform.localScale = rightTargetScale;
    }
}
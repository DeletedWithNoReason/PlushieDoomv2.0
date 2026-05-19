using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    public float Speed = 5f;

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 moveInput;
    private Vector2 lastLookDirection = new Vector2(0, -1);

    [Header("Integrations")]
    public DialogueManager dialogueManager; 
    public CutsceneManager cutsceneManager;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        if (dialogueManager == null) 
            dialogueManager = FindFirstObjectByType<DialogueManager>();
            
        if (cutsceneManager == null)
            cutsceneManager = FindFirstObjectByType<CutsceneManager>();
    }

    void Update()
    {
        bool busy = IsBusy();

        if (busy)
        {
            // Перевіряємо, чи існує клавіатура, перш ніж шукати на ній кнопку P
            if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
            {
                CheckWhoIsBlocking();
            }

            StopMovement();
            return;
        }

        HandleInput();
        UpdateAnimations();
    }

    private void CheckWhoIsBlocking()
    {
        Debug.Log("<color=orange>--- Movement Debug Check ---</color>");
        if (dialogueManager != null && dialogueManager.IsDialogueBusy()) Debug.Log("- DialogueManager is BUSY");
        if (cutsceneManager != null && cutsceneManager.IsBusy()) Debug.Log("- CutsceneManager is BUSY");
        if (ChoiceManager.Instance != null && ChoiceManager.Instance.IsBusy()) Debug.Log("- ChoiceManager is BUSY");
        if (EventManager.Instance != null && EventManager.Instance.IsEventRunning) Debug.Log("- EventManager is RUNNING");
        if (UIManagement.Instance != null && UIManagement.Instance.IsTransitioning) Debug.Log("- UIManagement is TRANSITIONING");
        
        // Перевірка на критичні компоненти
        if (rb == null) Debug.LogError("- Rigidbody2D is MISSING!");
        if (animator == null) Debug.LogError("- Animator is MISSING!");
    }

    private bool IsBusy()
    {
        return (dialogueManager != null && dialogueManager.IsDialogueBusy()) ||
            (cutsceneManager != null && cutsceneManager.IsBusy()) ||
            (ChoiceManager.Instance != null && ChoiceManager.Instance.IsBusy()) ||
            (EventManager.Instance != null && EventManager.Instance.IsEventRunning) ||
            (UIManagement.Instance != null && UIManagement.Instance.IsTransitioning);
    }

    void FixedUpdate()
    {
        rb.linearVelocity = moveInput * Speed;
    }

    private void HandleInput()
    {
        // ЗАХИСТ ВІД КРАШУ: Якщо клавіатури немає, просто виходимо
        if (Keyboard.current == null)
        {
            Debug.LogWarning("Клавіатуру не знайдено (Keyboard.current == null)! Рух неможливий.");
            return;
        }

        float x = 0;
        float y = 0;

        // Тепер це безпечно
        if (Keyboard.current.aKey.isPressed) x = -1;
        else if (Keyboard.current.dKey.isPressed) x = 1;
        else if (Keyboard.current.wKey.isPressed) y = 1;
        else if (Keyboard.current.sKey.isPressed) y = -1;

        moveInput = new Vector2(x, y);

        if (moveInput != Vector2.zero)
        {
            lastLookDirection = moveInput;
        }
    }

    private void UpdateAnimations()
    {
        animator.SetFloat("Horizontal", lastLookDirection.x);
        animator.SetFloat("Vertical", lastLookDirection.y);
        animator.SetFloat("Speed", moveInput.sqrMagnitude); 
    }

    private void StopMovement()
    {
        moveInput = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        animator.SetFloat("Speed", 0f); 
    }
}
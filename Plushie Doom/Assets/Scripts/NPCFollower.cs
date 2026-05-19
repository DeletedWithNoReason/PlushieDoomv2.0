using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(SpriteRenderer))]
public class NPCFollower : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 3.5f;
    public float startDistance = 2.5f;
    public float stopDistance = 1.0f;

    [Header("Sorting Settings")]
    public int orderInFront = 4;
    public int orderBehind = 2;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Transform player;

    private bool isFollowing = false;
    private Vector2 lastLookDirection = new Vector2(0, -1);

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        Movement playerMovement = FindFirstObjectByType<Movement>();
        if (playerMovement != null) player = playerMovement.transform;

        // Додамо дебаг, щоб побачити що відбувається в консолі
        if (EventManager.IsFollowingActive)
        {
            // Очищуємо ім'я від (Clone) для порівняння, якщо потрібно
            string cleanName = gameObject.name.Replace("(Clone)", "").Trim();
            
            if (cleanName == EventManager.ActiveFollowerPrefabName)
            {
                PositionNextToPlayer();
                SetFollowing(true);
                Debug.Log($"[NPC] {gameObject.name} успішно почав слідування.");
            }
        }
    }

    public void PositionNextToPlayer()
    {
        if (player == null) return;
        transform.position = new Vector3(player.position.x + 1f, player.position.y, player.position.z);
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    public void SetFollowing(bool follow)
    {
        isFollowing = follow;
        if (!isFollowing) StopMovement();
    }

    void FixedUpdate()
    {
        if (!isFollowing || player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance > startDistance)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.linearVelocity = direction * speed;

            lastLookDirection = direction;
            UpdateAnimations(direction.sqrMagnitude);
        }
        else if (distance <= stopDistance)
        {
            StopMovement();
        }
        else 
        {
            if (rb.linearVelocity.sqrMagnitude > 0.01f)
            {
                Vector2 direction = (player.position - transform.position).normalized;
                rb.linearVelocity = direction * speed;
                UpdateAnimations(direction.sqrMagnitude);
            }
            else
            {
                StopMovement();
            }
        }
    }

    // Використовуємо LateUpdate для коректного сортування після завершення руху
    void LateUpdate()
    {
        if (player != null)
        {
            UpdateSortingOrder();
        }
    }

    private void UpdateSortingOrder()
    {
        // Якщо NPC нижче гравця (Y менше), він має бути попереду (Layer 4)
        if (transform.position.y < player.position.y)
        {
            spriteRenderer.sortingOrder = orderInFront;
        }
        // Якщо NPC вище гравця (Y більше), він має бути позаду (Layer 2)
        else
        {
            spriteRenderer.sortingOrder = orderBehind;
        }
    }

    private void StopMovement()
    {
        if (rb != null) rb.linearVelocity = Vector2.zero;
        UpdateAnimations(0f);
    }

    private void UpdateAnimations(float movementSpeed)
    {
        animator.SetFloat("Horizontal", lastLookDirection.x);
        animator.SetFloat("Vertical", lastLookDirection.y);
        animator.SetFloat("Speed", movementSpeed);
    }
}
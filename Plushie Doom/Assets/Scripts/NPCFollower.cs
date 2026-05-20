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

        if (EventManager.IsFollowingActive)
        {
            string cleanName = gameObject.name.Replace("(Clone)", "").Trim();
            
            if (cleanName == EventManager.ActiveFollowerPrefabName)
            {
                PositionNextToPlayer();
                SetFollowing(true);
                Debug.Log($"[NPC] {gameObject.name} успішно почав слідування.");
            }
        }
        else
        {
            // Якщо NPC спавниться і НЕ слідує за гравцем — примусово дивиться вниз
            lastLookDirection = new Vector2(0, -1);
            UpdateAnimations(0f);
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
            MoveTowardsPlayer();
        }
        else if (distance <= stopDistance)
        {
            StopMovement();
        }
        else 
        {
            // Якщо він знаходиться між startDistance та stopDistance
            if (rb.linearVelocity.sqrMagnitude > 0.01f)
            {
                MoveTowardsPlayer();
            }
            else
            {
                StopMovement();
            }
        }
    }

    private void MoveTowardsPlayer()
    {
        // Сирий діагональний вектор для фізичного руху (щоб не застрягав на кутах)
        Vector2 rawDirection = (player.position - transform.position).normalized;
        rb.linearVelocity = rawDirection * speed;

        // Визначаємо домінуючу вісь для АНІМАЦІЇ (тільки 4 напрямки)
        Vector2 animDirection = Vector2.zero;
        if (Mathf.Abs(rawDirection.x) > Mathf.Abs(rawDirection.y))
        {
            animDirection.x = Mathf.Sign(rawDirection.x); // 1 або -1 по X
        }
        else
        {
            animDirection.y = Mathf.Sign(rawDirection.y); // 1 або -1 по Y
        }

        lastLookDirection = animDirection;
        UpdateAnimations(rb.linearVelocity.sqrMagnitude);
    }

    void LateUpdate()
    {
        if (player != null)
        {
            UpdateSortingOrder();
        }
    }

    private void UpdateSortingOrder()
    {
        if (transform.position.y < player.position.y)
        {
            spriteRenderer.sortingOrder = orderInFront;
        }
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
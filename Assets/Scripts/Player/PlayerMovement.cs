using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        Debug.Log("PlayerMovement Start called. Rigidbody2D? " + (rb != null) + ", Animator? " + (animator != null));
    }

    void Update()
    {
        // 取得玩家輸入（WASD）
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        moveInput = input.normalized;

        /* Debug.Log($"Input: {moveInput.x}, {moveInput.y}"); */

        animator.SetFloat("Horizontal", moveInput.x);
        animator.SetFloat("Vertical", moveInput.y);

        if (moveInput != Vector2.zero)
        {
            animator.SetFloat("LastHorizontal", moveInput.x);
            animator.SetFloat("LastVertical", moveInput.y);
        }
        
    }

    void FixedUpdate()
    {
        // 用 Rigidbody2D 移動角色
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }
}

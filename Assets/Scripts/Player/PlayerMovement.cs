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

        if (rb == null || animator == null)
        {
            Debug.LogError("PlayerMovement: Missing Rigidbody2D or Animator!");
        }

        // 🔹 一開始面向前方（front）
        animator.SetFloat("LastHorizontal", 0f);
        animator.SetFloat("LastVertical", -1f);
    }

    void Update()
    {
        // 如果對話正在進行，停下來不動
        if (DialogueManager.GetInstance() != null && DialogueManager.GetInstance().IsDialoguePlaying())
        {
            moveInput = Vector2.zero;
            animator.SetFloat("Horizontal", 0);
            animator.SetFloat("Vertical", 0);
            return;
        }

        // 從 InputManager 取得玩家輸入
        Vector2 input = InputManager.GetInstance().GetMovementDirection();
        moveInput = input.normalized;

        // 設定動畫參數
        animator.SetFloat("Horizontal", moveInput.x);
        animator.SetFloat("Vertical", moveInput.y);

        if (moveInput != Vector2.zero)
        {
            animator.SetFloat("LastHorizontal", moveInput.x);
            animator.SetFloat("LastVertical", moveInput.y);
        }
    }

    private void FixedUpdate()
    {

        /* if (DialogueManager.GetInstance().dialogueIsPlaying)
        {
            return;
        } */
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }
}

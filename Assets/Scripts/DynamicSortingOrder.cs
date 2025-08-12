using UnityEngine;

public class DynamicSortingOrder : MonoBehaviour
{
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // y 越小（越靠下），sortingOrder 越高，會蓋在其他物件上
        sr.sortingOrder = -Mathf.RoundToInt(transform.position.y * 100);
    }
}

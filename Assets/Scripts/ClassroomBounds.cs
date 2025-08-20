using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassroomBounds : MonoBehaviour
{
    [Header("邊界設定")]
    public float minX = -10.8f;
    public float maxX = 8.7f;
    public float minY = -10f;
    public float maxY = 4f;

    private void LateUpdate()
    {
        // 限制玩家位置在邊界內
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        transform.position = pos;
    }
}

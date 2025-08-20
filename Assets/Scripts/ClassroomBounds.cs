using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassroomBounds : MonoBehaviour
{
    [Header("邊界設定")]
    public float minX = -8.5f;
    public float maxX = 11f;
    public float minY = 3f;
    public float maxY = -11f;

    private void LateUpdate()
    {
        // 限制玩家位置在邊界內
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        transform.position = pos;
    }
}

using UnityEngine;

public class BlockMover : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed;
    public bool moveOnX = true;
    public float moveRange; // 왕복 범위 (-moveRange ~ +moveRange)

    private int _direction = 1;
    private bool _isMoving = true;

    void Update()
    {
        if (!_isMoving) return;

        // 이동 처리
        float move = moveSpeed * _direction * Time.deltaTime;

        if (moveOnX)
            transform.Translate(move, 0, 0);
        else
            transform.Translate(0, 0, move);

        // 범위 끝에 도달하면 방향 반전
        float currentPos = moveOnX ? transform.localPosition.x : transform.localPosition.z;
        if (currentPos >= moveRange || currentPos <= -moveRange)
        {
            _direction *= -1;
        }
    }

    // 외부에서 탭 시 호출 - 블록 멈춤
    public void Stop()
    {
        _isMoving = false;
    }
}

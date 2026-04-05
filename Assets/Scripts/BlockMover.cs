using UnityEngine;

public class BlockMover : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 3f;
    public float moveRange = 3f;
    public bool moveOnX = true;

    private int _direction = 1;
    private bool _isMoving = true;
    private float _startPos;

    void Start()
    {
        // 시작 위치 저장
        _startPos = moveOnX ? transform.position.x : transform.position.z;
    }

    void Update()
    {
        if (!_isMoving) return;

        // 이동 처리
        float move = moveSpeed * _direction * Time.deltaTime;

        if (moveOnX)
            transform.Translate(move, 0, 0);
        else
            transform.Translate(0, 0, move);


        // 범위 끝에 닿으면 방향 전환
        float currentPos = moveOnX ? transform.position.x : transform.position.z;
        if (Mathf.Abs(currentPos - _startPos) >= moveRange)
            _direction *= -1;

    }

    // 외부에서 탭 시 호출 - 블록 멈춤
    public void Stop()
    {
        _isMoving = false;
    }
}

using UnityEngine;

public class BlockMover : MonoBehaviour
{
    [Header("이동 설정")]
    public float _moveSpeed = 3f;
    public float _moveRange = 3f;
    public bool _moveOnX = true;

    private int _direction = 1;
    private bool _isMoving = true;
    private float _startPos;

    void Start()
    {
        // 시작 위치 저장
        _startPos = _moveOnX ? transform.position.x : transform.position.z;
    }

    void Update()
    {
        if (!_isMoving) return;

        // 이동 처리
        float move = _moveSpeed * _direction * Time.deltaTime;

        if (_moveOnX)
            transform.Translate(move, 0, 0);
        else
            transform.Translate(0, 0, move);


        // 범위 끝에 닿으면 방향 전환
        float currentPos = _moveOnX ? transform.position.x : transform.position.z;
        if (Mathf.Abs(currentPos - _startPos) >= _moveRange)
            _direction *= -1;

    }

    // 외부에서 탭 시 호출 - 블록 멈춤
    public void Stop()
    {
        _isMoving = false;
    }
}

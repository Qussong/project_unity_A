using UnityEngine;
using TMPro;
using System.IO;

public class StackManager : MonoBehaviour
{
    [Header("블록 설정")]
    public GameObject blockPrefab;
    public float blockWidth = 2f;       // 블록 너비 (x)
    public float blockLength = 2f;      // 블록 길이 (z)
    public float blockHeight = 1f;      // 블록 두께 (y)
    public float moveSpeed = 3f;
    public float blockPosDistance = 5f;

    [Header("UI")]
    public TextMeshProUGUI scoreText;

    // 내부 상태
    private Block _lastBlock;       // 직전에 놓인 블록
    private Block _currentBlock;    // 지금 움직이는 블록
    private int _score = 0;
    private bool _onX = true;  // 이동 축 (층마다 교대)
    private float _currentY = 0f;   // 다음 블록 Y 위치

    // 블록 색상 — 층마다 점진적으로 변함
    private Color _blockColor = Color.cyan;

    void Start()
    {
        SpawnBase();    // 바닥 고정 블록
        SpawnNext();    // 첫 번째 움직이는 블록
    }

    void Update()
    {
        bool isMouseTouched = Input.GetMouseButtonDown(0);
        bool isMobileTouched = Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;
        bool bTapped = isMouseTouched || isMobileTouched;

        if (bTapped && _currentBlock != null)
        {
            PlaceBlock();
        }

        // 생성된 블록이 기존 블록을 지나치면 게임 종료
        if (false)
        {
            GameOver();
        }
    }

    // 바닥 블록 (움직이지 않는 기준 블록) 생성
    void SpawnBase()
    {
        GameObject go = Instantiate(blockPrefab);
        _lastBlock = go.GetComponent<Block>();
        _lastBlock.Init(Vector3.zero,
                       new Vector3(blockWidth, blockHeight, blockLength),
                       Color.gray);
        _currentY = blockHeight;  // 다음 블록은 이 위에
    }

    // 다음 블록 생성
    void SpawnNext()
    {
        // 블록 색상 점진 변화 (HSV 회전)
        float h, s, v;
        Color.RGBToHSV(_blockColor, out h, out s, out v);
        h = (h + 0.05f) % 1f;
        _blockColor = Color.HSVToRGB(h, 0.7f, 0.95f);

        // 시작 위치 — 이동 축 반대편 끝에서 시작
        Vector3 startPos = new Vector3(
            _onX ? -blockPosDistance : _lastBlock.PosX,   // X축 이동이면 왼쪽 끝
            _currentY,
            _onX ? _lastBlock.PosZ : -blockPosDistance    // Z축 이동이면 앞쪽 끝
        );

        Vector3 scale = new Vector3(_lastBlock.Width,
                                    blockHeight,
                                    _lastBlock.Length);

        GameObject go = Instantiate(blockPrefab, startPos, Quaternion.identity);
        _currentBlock = go.GetComponent<Block>();
        _currentBlock.Init(startPos, scale, _blockColor);

        // 블록 이동 컴포넌트 세팅
        BlockMover mover = go.AddComponent<BlockMover>();
        // mover.moveRange = blockPosDistance * 2;
        mover.moveSpeed = moveSpeed + _score * 0.1f;  // 점수 오를수록 빨라짐
        mover.moveOnX = _onX;
    }

    // 탭 시: 겹침 계산 → 자르기 → 다음 블록 생성
    void PlaceBlock()
    {
        // 1. 현재 블록 이동 중단
        _currentBlock.GetComponent<BlockMover>().Stop();

        // 2. 이전 블록과의 위치 차이 (offset)
        float offset = _onX
            ? _currentBlock.PosX - _lastBlock.PosX
            : _currentBlock.PosZ - _lastBlock.PosZ;

        // 3. 겹침 크기 계산
        float blockSize = _onX ? _lastBlock.Width : _lastBlock.Length;
        float overlap = blockSize - Mathf.Abs(offset);

        // 4. 겹침이 없으면 게임오버
        if (overlap <= 0)
        {
            GameOver();
            return;
        }

        // 5-1. 완벽히 맞춘 경우 (0.1 이하 오차는 퍼펙트 처리)
        if (Mathf.Abs(offset) < 0.1f)
        {
            // 크기 유지
            overlap = blockSize;  

            // 위치 보정
            Vector3 pos = _currentBlock.transform.localPosition;
            if (_onX)
            {
                pos.x = _lastBlock.transform.localPosition.x;
            }
            else
            {
                pos.z = _lastBlock.transform.localPosition.z;
            }
            _currentBlock.transform.localPosition = pos;

            // 이펙트
            PerfectEffect();
        }
        // 5-2. 완벽히 맞추지 못하면
        else
        {
            float lastCenter = _onX
                ? _lastBlock.PosX
                : _lastBlock.PosZ;

            // 남은 블록의 새 중심 위치 계산
            float newCenter = lastCenter + offset * 0.5f;

            // 조각 크기
            float debrisSize = _onX 
                               ? _currentBlock.Width - overlap 
                               : _currentBlock.Length - overlap;

            // 블록 자르기 실행
            _currentBlock.Slice(overlap, newCenter, _onX);

            // 조각 블록 생성
            _currentBlock.SpawnDebris(debrisSize, lastCenter, newCenter, _onX);

        }

        // 6. 다음 블록 준비
        _score++;
        UpdateScore();

        _lastBlock = _currentBlock;
        _currentBlock = null;
        _currentY += blockHeight;
        _onX = !_onX;   // 다음 층은 반대 축으로 이동

        // 카메라 올리기
        MoveCamera();

        // 다음 블록 생성
        SpawnNext();
    }

    // 퍼펙트 판정 시각 피드백
    void PerfectEffect()
    {
        Debug.Log("Perfect");
    }

    // 카메라를 블록 높이에 맞춰 올림
    void MoveCamera()
    {
        Camera.main.transform.position += new Vector3(0, blockHeight, 0);
    }

    // 점수 업데이트
    void UpdateScore()
    {
        if (scoreText != null)
            scoreText.text = _score.ToString();
    }

    // 게임오버
    void GameOver()
    {
        Debug.Log("Game Over! Score: " + _score);
        // 다음 단계에서 씬 전환 추가
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
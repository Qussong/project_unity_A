using DG.Tweening;
using PixelBattleText;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public enum EPlaceState
{
    GOOD,
    BAD,
    PERFECT,
    NONE
}

public class StackManager : MonoBehaviour
{
    [Header("Setting")]
    public GameObject objBlockContainer;// 생성된 블록의 부모 오브젝트
    public GameObject objBaseBlock;     // 첫번째 블록

    [Header("블록 설정")]
    public GameObject blockPrefab;
    // public float blockWidth = 1f;       // 블록 너비 (x)
    // public float blockLength = 1f;      // 블록 길이 (z)
    public float blockHeight = 0.2f;    // 블록 두께 (y)
    public float minMoveSpeed = 2f;     // 시작 속도
    public float maxMoveSpeed = 5f;     // 최대 속도
    public float blockPosDistance = 2f;

    [Header("UI")]
    public UIManager uiManager;

    [Header("Effect")]
    public ParticleSystem stackEffectPrefab;

    [Header("PixelBattleText")]
    public TextAnimation taGood;
    public TextAnimation taBad;
    public TextAnimation taPerfect;
    public string strGood;
    public string strBad;
    public string strPerfect;
    private EPlaceState _placeState = EPlaceState.NONE;

    // 내부 상태
    private Block _lastBlock;       // 직전에 놓인 블록
    private Block _currentBlock;    // 지금 움직이는 블록
    private int _score = 0;
    private bool _onX = true;       // 이동 축 (층마다 교대)
    private float _currentY = 0f;   // 다음 블록 Y 위치
    private float _initBlockWidth;  // 초기 블록 너비 (x)
    private float _initBlockLength; // 초기 블록 길이 (z)

    // 블록 색상 — 층마다 점진적으로 변함
    private Color _blockColor = Color.cyan;

    // 카메라 위치 변경 플래그
    private bool _bMoveCamera = false;

    // 게임 플레이 플래그
    private bool _bPlayGame = false;

    private Camera _mainCam = null;
    private Vector3 _camInitPos = Vector3.zero;


    void Start()
    {
        // 메인 카메라 캐싱
        _mainCam = Camera.main;
        // 카메라 위치 저장
        _camInitPos = _mainCam.transform.localPosition;

        // 초기 블록 크기 저장
        _initBlockWidth = objBaseBlock.transform.localScale.x;
        _initBlockLength = objBaseBlock.transform.localScale.z;

        // 게임 플레이 버튼 이벤트 연결
        uiManager.btnStartGame.onClick.AddListener(StartGame);

        // 재시작 버튼 이벤트 연결
        uiManager.btnRestart.onClick.AddListener(ResetGame);

        SetupBaseBlock();   // 바닥 고정 블록 설정
        // SpawnNext();     // 첫 번째 움직이는 블록
    }

    void Update()
    {
        // 게임 플레이 상태 아니면 탭 인식 x
        if (false == _bPlayGame) return;

        bool isMouseTouched = Input.GetMouseButtonDown(0) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
        bool isMobileTouched = Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        bool test = Input.GetKeyDown(KeyCode.Space);
        bool bTapped = isMouseTouched || isMobileTouched || test;

        if (bTapped && _currentBlock != null)
        {
            PlaceBlock();
        }

    }

    void LateUpdate()
    {
        if (_bMoveCamera)
        {
            _bMoveCamera = false;
            MoveCamera();
        }
    }

    private void StartGame()
    {
        // 홈 화면 패널 숨기기
        uiManager.panelHome.SetActive(false);

        // 배경음 줄이기
        SoundManager.Instance.FadeBGMVolume(0.15f, 1f);

        _bPlayGame = true;  // 게임 플레이 플래그 on
        SpawnNext();        // 첫 번째 움직이는 블록
    }

    // 바닥 블록 (움직이지 않는 기준 블록) 생성
    void SetupBaseBlock()
    {
        _lastBlock = objBaseBlock.AddComponent<Block>();
        objBaseBlock.GetComponent<Renderer>().material.color = Color.white;
        _currentY = blockHeight / 2f;  // 다음 블록 Y 위치
    }

    // 다음 블록 생성
    void SpawnNext()
    {
        // 블록 색상 점진 변화 (HSV 회전)
        float h, s, v;
        Color.RGBToHSV(_blockColor, out h, out s, out v);
        // h = (h + 0.05f) % 1f;
        // _blockColor = Color.HSVToRGB(h, 0.7f, 0.95f);
        h = (h + 0.07f) % 1f;
        _blockColor = Color.HSVToRGB(h, 0.35f, 0.98f);

        // 시작 위치 — 이동 축 반대편 끝에서 시작
        Vector3 startPos = new Vector3(
            _onX ? -blockPosDistance : _lastBlock.PosX,   // X축 이동이면 왼쪽 끝
            _currentY,
            _onX ? _lastBlock.PosZ : -blockPosDistance    // Z축 이동이면 앞쪽 끝
        );

        Vector3 scale = new Vector3(_lastBlock.Width,
                                    blockHeight,
                                    _lastBlock.Length);

        GameObject go = Instantiate(blockPrefab, startPos, Quaternion.identity, objBlockContainer.transform);
        _currentBlock = go.GetComponent<Block>();
        _currentBlock.Init(startPos, scale, _blockColor);

        // 블록 이동 컴포넌트 세팅
        BlockMover mover = go.AddComponent<BlockMover>();
        // 점수 오를수록 빨라짐
        mover.moveSpeed = Mathf.Min(minMoveSpeed + _score * 0.01f, maxMoveSpeed);
        mover.moveOnX = _onX;
        mover.moveRange = blockPosDistance;
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
        
        float offectPercent = Mathf.Abs(offset) / blockSize;
        // 5-1. 완벽히 맞춘 경우 (10% 미만의 오차는 퍼펙트 처리)
        if (offectPercent < 0.1f)
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

            // 매칭 상태
            _placeState = EPlaceState.PERFECT;
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

            // 매칭 상태
            _placeState = offectPercent < 0.3f ? EPlaceState.GOOD : EPlaceState.BAD;
        }

        // 효과음 재생
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clipBlockPlace);

        // 떡 효과
        _currentBlock.PlayBounceEffect();
        // BattleTextAnimation 출력
        DisplayPixelBattelText(_placeState);
        _placeState = EPlaceState.NONE;

        // 점수 갱신
        _score++;
        // UpdateScore();
        uiManager.SetScore(_score);

        // 다음 블록 준비
        _lastBlock = _currentBlock;
        _currentBlock = null;
        _currentY += blockHeight;
        _onX = !_onX;   // 다음 층은 반대 축으로 이동

        // 카메라 올리기
        _bMoveCamera = true;

        // 다음 블록 생성
        SpawnNext();
    }

    // 퍼펙트 판정 시각 피드백
    void PerfectEffect()
    {
        Debug.Log("Perfect");
        // 이펙트 
        PlayStackEffect(_currentBlock.transform.localPosition);
    }

    // 카메라를 블록 높이에 맞춰 올림
    void MoveCamera()
    {
        Vector3 target = _mainCam.transform.position + new Vector3(0, blockHeight, 0);
        _mainCam.transform.DOMove(target, 0.3f).SetEase(Ease.OutCubic);
    }

    // 게임오버
    void GameOver()
    {
        Debug.Log("Game Over!");

        // 게임 플레이 플래그 off
        _bPlayGame = false;

        // 현재 블록 이동 정지
        _currentBlock.GetComponent<BlockMover>().Stop();
        _currentBlock = null;

        // 게임 종료 패널 표시
        uiManager.ShowGameOver(_score);

        // 배경음 높이기
        SoundManager.Instance.FadeBGMVolume(0.5f, 0.5f);
    }

    private void PlayStackEffect(Vector3 position)
    {
        var main = stackEffectPrefab.main;
        main.duration = 0.2f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.5f, 1f);
        main.startRotation = new ParticleSystem.MinMaxCurve(-180f * Mathf.Deg2Rad, 180f * Mathf.Deg2Rad);
        main.playOnAwake = false;

        var shape = stackEffectPrefab.shape;
        shape.radius = 0.5f;

        var emission = stackEffectPrefab.emission;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0f, 6)
        });

        ParticleSystem effect = Instantiate(stackEffectPrefab, position, Quaternion.Euler(0, 0, 0), objBlockContainer.transform);
        effect.Play();

        Destroy(effect.gameObject, 1.5f);
    }

    private void ResetGame()
    {
        // 게임오버 패널 숨기기
        uiManager.panelGameOver.SetActive(false);

        // 홈 화면 패널 표출
        uiManager.panelHome.SetActive(true);

        // 쌓아둔 블록 전부 제거
        foreach (Transform block in objBlockContainer.transform)
            Destroy(block.gameObject);

        // 상태 값 초기화
        _score = 0;
        uiManager.SetScore(_score);
        _onX = true;
        _currentY = 0f;
        _currentBlock = null;
        _blockColor = Color.cyan;

        // 카메라 위치 리셋
        _mainCam.transform.localPosition = _camInitPos;

        // baseBlock 재설정
        SetupBaseBlock();
    }

    private void DisplayPixelBattelText(EPlaceState state)
    {
        Vector2 viewportPosition = Camera.main.WorldToViewportPoint(_currentBlock.transform.localPosition + new Vector3(0f,0.5f,0f));
        viewportPosition.x = 0.5f;

        if(state == EPlaceState.GOOD)
        {
            PixelBattleTextController.DisplayText(strGood, taGood, viewportPosition);
        }
        else if(state == EPlaceState.BAD)
        {
            PixelBattleTextController.DisplayText(strBad, taBad, viewportPosition);
        }
        else if(state == EPlaceState.PERFECT)
        {
            PixelBattleTextController.DisplayText(strPerfect, taPerfect, viewportPosition);
        }
        else
        {
            //
        }
    }

}
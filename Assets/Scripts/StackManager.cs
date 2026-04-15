using System;
using DG.Tweening;
using PixelBattleText;
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
    public float blockHeight = 0.2f;    // 블록 두께 (y)
    public float minMoveSpeed = 2f;     // 시작 속도
    public float maxMoveSpeed = 5f;     // 최대 속도
    // public float spawnDistance = 2f;

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

    // 블록 색상 — 층마다 점진적으로 변함
    private Color _blockColor = Color.cyan;

    // 카메라 위치 변경 플래그
    private bool _bMoveCamera = false;

    // 게임 플레이 플래그
    private bool _bPlayGame = false;

    // 메인 카메라 정보 캐싱
    private Camera _mainCam = null;
    private Vector3 _camInitPos = Vector3.zero;
    private float _camInitOrthoSize = 0;

    // Perfect 콤보
    private int _comboCnt = 0;
    private float _nextBlockScaleX = 0f;
    private float _nextBlockScaleZ = 0f;
    private const float _comboGrowthRate = 1.1f;  // 콤보 성장률 10%
    private const float _maxComboGrowth = 0.1f;  // 콤보당 최대 성장량

    // 카메라 이동
    private float _moveCenterX = 0f;
    private float _moveCenterZ = 0f;


    void Start()
    {
        // 메인 카메라 캐싱
        _mainCam = Camera.main;
        // 카메라 위치 저장
        _camInitPos = _mainCam.transform.position;
        _camInitOrthoSize = _mainCam.orthographicSize;

        // 게임 플레이 버튼 이벤트 연결
        uiManager.btnStartGame.onClick.AddListener(StartGame);

        // 재시작 버튼 이벤트 연결
        uiManager.btnRestart.onClick.AddListener(ResetGame);

        SetupBaseBlock();   // 바닥 고정 블록 설정

        // 콤보 설정 초기화
        _comboCnt = 0;
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

        // 디음 블록 스케일 캐싱
        _nextBlockScaleX = _lastBlock.Width;
        _nextBlockScaleZ = _lastBlock.Length;
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
            // 매칭 상태
            _placeState = EPlaceState.PERFECT;

            _comboCnt += 1;

            // 크기 유지
            overlap = blockSize;

            // 위치 보정
            Vector3 pos = _currentBlock.transform.localPosition;
            if (_onX)
                pos.x = _lastBlock.transform.localPosition.x;
            else
                pos.z = _lastBlock.transform.localPosition.z;
            _currentBlock.transform.localPosition = pos;

            // 이펙트
            PerfectEffect();
        }
        // 5-2. 완벽히 맞추지 못하면
        else
        {
            // 매칭 상태
            _placeState = offectPercent < 0.3f ? EPlaceState.GOOD : EPlaceState.BAD;
            _comboCnt = 0;

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

        // 카메라 프로젝션 사이즈 블록 사이즈에 맞게 조정 (블록이 커지면 Projection 사이즈 증가)
        FitCameraProjectionSizeToBlock(_currentBlock);

        // 효과음 재생
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clipBlockPlace);

        // 떡 효과
        _currentBlock.PlayBounceEffect();
        DisplayPixelBattelText(_placeState);    // BattleTextAnimation 출력
        _placeState = EPlaceState.NONE;

        // 점수 갱신
        _score++;
        uiManager.SetScore(_score);

        // 다음 블록의 사이즈 캐싱
        _nextBlockScaleX = _currentBlock.Width;
        _nextBlockScaleZ = _currentBlock.Length;

        // 콤보 보상
        if (_comboCnt >= 3)
        {
            ApplyComboBonus();
            _comboCnt = 0;
        }

        // 중심축 이동량 캐싱, 슬라이싱으로 블록 중심이 이동한 만큼 카메라 X/Z도 보정
        _moveCenterX = _currentBlock.transform.position.x - _lastBlock.transform.position.x;
        _moveCenterZ = _currentBlock.transform.position.z - _lastBlock.transform.position.z;

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

    private void FitCameraProjectionSizeToBlock(Block block)
    {
        Renderer r = block.GetComponent<Renderer>();
        Bounds b = r.bounds;
        Vector3[] outlinePoints = new Vector3[]
        {
            new Vector3(b.max.x, b.min.y, b.min.z),
            new Vector3(b.max.x, b.max.y, b.min.z),
            new Vector3(b.min.x, b.max.y, b.max.z),
            new Vector3(b.min.x, b.min.y, b.max.z),
        };

        // 디버깅용 Cube 생성 로직
        /* int idx = 0;
        foreach (Vector3 pos in outlinePoints)
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.transform.SetParent(objBlockContainer.transform);
            marker.name = "Debris";
            marker.GetComponent<Renderer>().material.color = Color.red;
            marker.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            marker.transform.position = pos;

            Debug.Log($"[{idx++}] : {_mainCam.WorldToViewportPoint(pos)}");
        } */


        // 카메라 Projection Size 변경
        foreach (Vector3 pos in outlinePoints)
        {
            Vector3 vp = _mainCam.WorldToViewportPoint(pos);
            if (vp.x > 1f || vp.x < 0f)
            {
                // Debug.Log("Camera Projection Size UP!!!");
                float curSize = _mainCam.orthographicSize;
                _mainCam.DOOrthoSize(++curSize, 1f).SetEase(Ease.OutCubic);
                return;
            }
        }
    }

    private void ApplyComboBonus()
    {
        if (_onX)
        {
            // float bonusScaleX = _currentBlock.Width * _comboGrowthRate;
            // float offsetX = bonusScaleX - _currentBlock.Width;
            // float growthX = Mathf.Min(offsetX, _maxComboGrowth);
            // float nextBlockScaleX = _currentBlock.Width + growthX;
            float growthX = Mathf.Min(_currentBlock.Width * (_comboGrowthRate - 1f), _maxComboGrowth);
            _nextBlockScaleX = _currentBlock.Width + growthX;
            // float targetX = Mathf.Min(_currentBlock.Width * _comboGrowthRate, _maxComboGrowth);
            // _nextBlockScaleX = targetX;  // 콤보 보상으로 커진 블록의 X 스케일 캐싱
            // _nextBlockScaleX = nextBlockScaleX;  // 콤보 보상으로 커진 블록의 X 스케일 캐싱
            _currentBlock.transform.DOScaleX(_nextBlockScaleX, 0.4f).SetEase(Ease.OutElastic);
        }
        else
        {
            // float bonusScaleZ = _currentBlock.Length * _comboGrowthRate;
            // float offsetZ = bonusScaleZ - _currentBlock.Length;
            // float growthZ = Mathf.Min(offsetZ, _maxComboGrowth);
            // float nextBlockScaleZ = _currentBlock.Length + growthZ;
            float growthZ = Mathf.Min(_currentBlock.Length * (_comboGrowthRate - 1f), _maxComboGrowth);
            _nextBlockScaleZ = _currentBlock.Length + growthZ;
            // float targetZ = Mathf.Min(_currentBlock.Length * _comboGrowthRate, _maxComboGrowth);
            // _nextBlockScaleZ = targetZ;  // 콤보 보상으로 커진 블록의 Z 스케일 캐싱
            // _nextBlockScaleZ = nextBlockScaleZ;  // 콤보 보상으로 커진 블록의 Z 스케일 캐싱
            _currentBlock.transform.DOScaleZ(_nextBlockScaleZ, 0.4f).SetEase(Ease.OutElastic);
        }
    }

    // 퍼펙트 판정 시각 피드백
    void PerfectEffect()
    {
        Debug.Log("Perfect");
        // 이펙트 
        PlayStackEffect(_currentBlock.transform.localPosition);
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
            new ParticleSystem.Burst(0f, _comboCnt * 2f)
        });

        ParticleSystem effect = Instantiate(stackEffectPrefab, position, Quaternion.Euler(0, 0, 0), objBlockContainer.transform);
        effect.Play();

        Destroy(effect.gameObject, 1.5f);
    }

    // 다음 블록 생성
    void SpawnNext()
    {
        // 블록 색상 점진 변화 (HSV 회전)
        Color.RGBToHSV(_blockColor, out float h, out _, out _);
        h = (h + 0.07f) % 1f;
        _blockColor = Color.HSVToRGB(h, 0.35f, 0.98f);

        // 블록 스폰 거리
        float spawnDistance = Mathf.Max(_onX ? _nextBlockScaleX * 2f : _nextBlockScaleZ * 2f, 2f);

        // 시작 위치 — 이동 축 반대편 끝에서 시작
        Vector3 startPos = new Vector3(
            _onX ? -spawnDistance : _lastBlock.PosX,   // X축 이동이면 왼쪽 끝
            _currentY,
            _onX ? _lastBlock.PosZ : -spawnDistance    // Z축 이동이면 앞쪽 끝
        );

        Vector3 scale = new Vector3(_nextBlockScaleX,
                                    blockHeight,
                                    _nextBlockScaleZ);

        GameObject go = Instantiate(blockPrefab, startPos, Quaternion.identity, objBlockContainer.transform);
        _currentBlock = go.GetComponent<Block>();
        _currentBlock.Init(startPos, scale, _blockColor);

        // 블록 이동 컴포넌트 세팅
        BlockMover mover = go.AddComponent<BlockMover>();
        
        // 점수 오를수록 빨라짐
        mover.moveSpeed = Mathf.Min(minMoveSpeed + _score * 0.01f, maxMoveSpeed);
        mover.moveOnX = _onX;
        mover.moveRange = spawnDistance;
    }

    // 카메라를 블록 높이에 맞춰 올림
    void MoveCamera()
    {
        Vector3 target = _mainCam.transform.position + new Vector3(_moveCenterX, blockHeight, _moveCenterZ);
        _mainCam.transform.DOMove(target, 0.5f).SetEase(Ease.OutCubic);

        // 이동량 캐싱 변수 초기화
        _moveCenterX = 0f;
        _moveCenterZ = 0f;
    }

    // 게임오버
    void GameOver()
    {
        Debug.Log("Game Over!");

        // 게임 플레이 플래그 off
        _bPlayGame = false;

        // 카메라 이동
        ShowFullStack();

        // 현재 블록 이동 정지
        _currentBlock.GetComponent<BlockMover>().Stop();
        _currentBlock = null;


        // 콤보 값 초기화
        _comboCnt = 0;

        // 게임 종료 패널 표시
        uiManager.ShowGameOver(_score);

        // 배경음 높이기
        SoundManager.Instance.FadeBGMVolume(0.5f, 0.5f);
    }

    private void ShowFullStack()
    {
        // 스택의 월드 Y 범위: 바닥(0) ~ 최상단 블록 top
        float stackBottomY = -1f;
        float stackTopY = _currentBlock.transform.position.y + _currentBlock.transform.localScale.y / 2f;

        // 바닥·꼭대기 점을 뷰포트 좌표(0~1)로 변환
        Vector3 bottomVP = _mainCam.WorldToViewportPoint(new Vector3(0f, stackBottomY, 0f));
        Vector3 topVP = _mainCam.WorldToViewportPoint(new Vector3(0f, stackTopY, 0f));

        // 현재 뷰포트에서 스택이 차지하는 Y 비율
        float currentSpan = topVP.y - bottomVP.y;
        // 목표 비율: 화면 높이의 75%를 스택이 채우도록
        float desiredSpan = 0.75f;

        // orthographicSize를 비례 축소하면 동일한 월드 범위가 더 큰 뷰포트 비율을 차지함
        // targetSize = currentSize × (currentSpan / desiredSpan)
        // ex) currentSpan=0.3, desiredSpan=0.75 → targetSize = currentSize × 0.4 (줌인)
        float targetSize = _mainCam.orthographicSize * (currentSpan / desiredSpan);

        // 스택의 월드 중심점
        Vector3 stackMid = new Vector3(0f, (stackBottomY + stackTopY) / 2f, 0f);

        // stackMid 까지의 카메라 전방(forward) 거리 — ViewportToWorldPoint의 z 입력값으로 사용
        float depth = _mainCam.WorldToViewportPoint(stackMid).z;

        // 현재 화면 정중앙(0.5, 0.5)에 해당하는 월드 좌표 역산
        // → 카메라 각도와 무관하게 동작 (투영 역변환)
        Vector3 currentViewCenter = _mainCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, depth));

        // 화면 중심을 stackMid로 옮기는 데 필요한 카메라 이동량
        Vector3 targetCamPos = _mainCam.transform.position + (stackMid - currentViewCenter);

        // 카메라 위치·크기 동시 애니메이션
        _mainCam.transform.DOMove(targetCamPos, 1f).SetEase(Ease.OutCubic);
        _mainCam.DOOrthoSize(targetSize, 1f).SetEase(Ease.OutCubic);
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

        // 카메라 위치 리셋 - 진행 중인 트윈을 먼저 종료 후 초기 위치로 복원
        _mainCam.transform.DOKill();
        _mainCam.DOKill();
        _mainCam.transform.position = _camInitPos;
        _mainCam.orthographicSize = _camInitOrthoSize;

        // baseBlock 재설정
        SetupBaseBlock();
    }

    private void DisplayPixelBattelText(EPlaceState state)
    {
        Vector2 viewportPosition = Camera.main.WorldToViewportPoint(_currentBlock.transform.localPosition + new Vector3(0f, 0.5f, 0f));
        viewportPosition.x = 0.5f;

        if (state == EPlaceState.GOOD)
        {
            PixelBattleTextController.DisplayText(strGood, taGood, viewportPosition);
        }
        else if (state == EPlaceState.BAD)
        {
            PixelBattleTextController.DisplayText(strBad, taBad, viewportPosition);
        }
        else if (state == EPlaceState.PERFECT)
        {
            PixelBattleTextController.DisplayText(strPerfect, taPerfect, viewportPosition);
        }
        else
        {
            //
        }
    }



}
using UnityEngine;
using UnityEngine.AI; // NavMeshAgent 사용을 위해 필요

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class SparrowPatrol : MonoBehaviour
{
    [Header("Patrol Area")]
    public Vector3 areaCenter = Vector3.zero;       // 순찰 영역의 중심 위치
    public Vector3 areaSize = new Vector3(10f, 0f, 10f); // 순찰 영역 크기 (Y=0: 지면 위 이동)

    [Header("Movement")]
    public float moveSpeed = 3f;         // 이동 속도
    public float waitTimeMin = 0.5f;     // 목적지 도착 후 최소 대기 시간
    public float waitTimeMax = 2f;       // 목적지 도착 후 최대 대기 시간
    public float arrivalDistance = 0.5f; // 이 거리 이하면 도착으로 판단 (NavMeshAgent stoppingDistance와 맞춰야 함)

    private NavMeshAgent _agent;    // 경로 계산 및 이동을 담당하는 컴포넌트
    private Animator _animator;
    private static readonly int IsWalking = Animator.StringToHash("isWalking");

    private float _waitTimer = 0f;
    private bool _isWaiting = false;

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();

        // NavMeshAgent 속도를 Inspector 설정값과 동기화
        _agent.speed = moveSpeed;

        // stoppingDistance: 목적지에서 이 거리만큼 남으면 Agent가 스스로 멈춤
        _agent.stoppingDistance = arrivalDistance;

        PickNewTarget();
    }

    void Update()
    {
        if (_isWaiting)
        {
            // 대기 중 → idle 애니메이션
            _animator.SetBool(IsWalking, false);

            _waitTimer -= Time.deltaTime;
            if (_waitTimer <= 0f)
            {
                _isWaiting = false;
                PickNewTarget(); // 새 목적지 선택 후 이동 재개
            }
            return;
        }

        // 이동 중 → walk 애니메이션
        _animator.SetBool(IsWalking, true);

        // pathPending: 경로 계산 중이면 true (계산 완료 전엔 remainingDistance가 0으로 나와서 오판 방지)
        if (!_agent.pathPending && _agent.remainingDistance <= arrivalDistance)
        {
            _isWaiting = true;
            _waitTimer = Random.Range(waitTimeMin, waitTimeMax);
        }
    }

    void PickNewTarget()
    {
        // 순찰 영역 안에서 랜덤 위치 선택
        Vector3 randomPos = areaCenter + new Vector3(
            Random.Range(-areaSize.x / 2f, areaSize.x / 2f),
            0f, // 지면 위를 걷는 경우 Y는 고정 (공중 이동이면 areaSize.y 적용)
            Random.Range(-areaSize.z / 2f, areaSize.z / 2f)
        );

        // NavMesh 위의 유효한 위치인지 샘플링 (장애물 위나 NavMesh 밖이면 보정)
        if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            _agent.SetDestination(hit.position); // 유효한 위치로 이동 명령
        }
        else
        {
            // 유효한 위치를 못 찾으면 중심으로 이동
            _agent.SetDestination(areaCenter);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawCube(areaCenter, areaSize);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(areaCenter, areaSize);
    }
}

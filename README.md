# ProjectA

> 하이퍼캐주얼 게임 **Stack** 모작 — Unity로 구현하는 블록 쌓기 게임

`[📷 이미지 배치 예정 - 게임 플레이 화면]`

| 항목 | 내용 |
|---|---|
| 장르 | 하이퍼캐주얼 / 퍼즐 |
| 플랫폼 | PC · Mobile |
| 엔진 | Unity 6 (URP) |
| 언어 | C# |
| 버전 | 0.2.0 |

---

## 화면 구성

| 게임 화면 |
|---|
| `[📷 이미지 배치 예정 - 메인 게임 플레이]` |

```
[시작] → [게임 플레이] → [게임 오버]
                ↑               |
                └───────────────┘ (씬 재로드)
```

| 화면 | 설명 |
|---|---|
| 게임 플레이 | 블록 이동 → 탭으로 정지 → 겹침 판정 → 슬라이싱 반복 |
| 게임 오버 | 블록을 완전히 놓쳤을 때 씬 재로드로 재시작 |

- 게임 오버 시 현재 씬을 즉시 재로드하여 재시작

---

## 핵심 기능

| 기능 | 설명 |
|---|---|
| 블록 자동 이동 | X축·Z축 교대로 왕복 이동, 점수에 비례해 속도 증가 (minMoveSpeed~maxMoveSpeed 범위, 선형 증가) |
| 탭 입력 감지 | 마우스 클릭·모바일 터치 통합 처리 후 블록 정지 |
| 블록 슬라이싱 | 겹친 영역만 잔류, 초과 부분은 Debris로 분리 후 중력 낙하 |
| 퍼펙트 판정 | 오차 0.1 이내 시 크기 유지 및 위치 자동 보정 |
| 누적 스택 | 잔류 블록 위에 다음 블록 생성, 카메라 자동 상승 |
| 색상 변화 | HSV 색상환을 따라 층마다 블록 색상 점진 변화 |
| 점수 UI | TextMeshPro로 현재 점수 실시간 표시 |

---

## 아키텍처

단일 씬 기반 심플 구성. StackManager가 게임 루프를 총괄하고, 블록 이동은 BlockMover에 위임.

| 레이어 | 역할 | 주요 클래스 |
|---|---|---|
| 게임 매니저 | 입력 감지, 블록 생성·배치, 슬라이싱 판정, 점수·게임오버 관리, 속도 상한 제어 | `StackManager` |
| 블록 데이터 | 블록 크기·위치 관리, 슬라이싱 연산, Debris 생성 | `Block` |
| 블록 이동 | X/Z축 왕복 이동, 정지 처리 | `BlockMover` |
| 유틸리티 | 블록 색상 설정 | `ColorModifier` |
| UI | 점수 표시 | `TextMeshProUGUI` (StackManager에서 참조) |

---

## 디렉토리 구조

```
ProjectA/
├── Assets/
│   ├── Prefabs/
│   │   └── Cube.prefab                # 블록 프리팹
│   ├── Scenes/
│   │   └── ProjectA.unity             # 메인 씬
│   ├── Scripts/
│   │   ├── StackMaanger.cs            # 게임 루프 총괄 매니저
│   │   ├── Block.cs                   # 블록 데이터·슬라이싱 로직
│   │   ├── BlockMover.cs              # 블록 이동·정지 처리
│   │   ├── ColorModifier.cs           # 블록 색상 설정
│   │   └── InputManager.cs            # 입력 감지 (미사용, StackManager로 이관)
│   └── Settings/
│       ├── Mobile_RPAsset.asset       # URP 모바일 설정
│       └── PC_RPAsset.asset           # URP PC 설정
├── Packages/
└── ProjectSettings/
```

---

## 데이터 흐름

```
플레이어 입력 (클릭 / 터치)
    └─▶ StackManager.PlaceBlock()
            ├─▶ BlockMover.Stop()          — 블록 정지
            ├─▶ 겹침(overlap) 계산
            │       ├─ overlap ≤ 0         → GameOver (씬 재로드)
            │       ├─ offset < 0.1        → 퍼펙트 판정 (크기 유지)
            │       └─ 그 외              → Block.Slice() → Debris 낙하
            └─▶ SpawnNext()                — 다음 블록 생성
```

---

## 변경 이력

| 날짜 | 내용 |
|---|---|
| 2026-04-06 | 난이도 속도 증가에 상한선 추가 (minMoveSpeed·maxMoveSpeed Inspector 노출), 선형 증가 계수 0.1→0.05 조정 |
| 2026-04-06 | StackManager·Block 핵심 로직 구현 (슬라이싱, 퍼펙트 판정, 점수 UI, 카메라 추적) |
| 2026-04-04 | 프로젝트 초기 세팅, BlockMover 이동·정지 구현, InputManager 추가 |

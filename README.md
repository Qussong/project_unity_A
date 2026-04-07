# ProjectA

> 하이퍼캐주얼 게임 **Stack** 모작 — Unity로 구현하는 블록 쌓기 게임

`[📷 이미지 배치 예정 - 게임 플레이 화면]`

| 항목 | 내용 |
|---|---|
| 장르 | 하이퍼캐주얼 / 퍼즐 |
| 플랫폼 | PC · Mobile |
| 엔진 | Unity 6 (URP) |
| 언어 | C# |
| 버전 | 0.3.0 |

---

## 화면 구성

| 게임 화면 | 게임오버 패널 |
|---|---|
| `[📷 이미지 배치 예정 - 메인 게임 플레이]` | `[📷 이미지 배치 예정 - 게임오버 패널]` |

```
[게임 시작] → [블록 이동 중] → [탭 입력]
                                    |
                  ┌─────────────────┴──────────────────┐
                  │ 겹침 있음                           │ 겹침 없음 / 경계 이탈
                  ▼                                     ▼
           [슬라이싱 후 다음 블록]               [게임오버 패널]
                  ↑                                     |
                  └─────────────────────────────────────┘
                                              (Restart → 씬 재로드)
```

| 화면 | 설명 |
|---|---|
| 게임 플레이 | 블록 이동 → 탭으로 정지 → 겹침 판정 → 슬라이싱 반복, 점수·최고점수 실시간 표시 |
| 게임오버 패널 | 최종 점수 표시 + Restart 버튼으로 씬 재로드 |

- 최고 점수는 `PlayerPrefs`로 기기에 영구 저장됨

---

## 핵심 기능

| 기능 | 설명 |
|---|---|
| 블록 자동 이동 | X축·Z축 교대로 단방향 이동, 점수에 비례해 속도 증가 (`minMoveSpeed`~`maxMoveSpeed`, 선형 증가) |
| 탭 입력 감지 | 마우스 클릭·모바일 터치·스페이스바 통합 처리 후 블록 정지 |
| 블록 슬라이싱 | 겹친 영역만 잔류, 초과 부분은 Debris로 분리 후 중력 낙하 |
| 퍼펙트 판정 | 오차 0.1 이내 시 크기 유지 및 위치 자동 보정 |
| 경계 이탈 감지 | 현재 블록이 이전 블록 범위를 완전히 벗어나면 즉시 게임오버 |
| 누적 스택 | 잔류 블록 위에 다음 블록 생성, 카메라 자동 상승 |
| 색상 변화 | HSV 색상환을 따라 층마다 블록 색상 점진 변화 |
| 점수 UI | 현재 점수·최고 점수 실시간 표시 (TextMeshPro) |
| 게임오버 패널 | 최종 점수 표시, Restart 버튼으로 씬 재로드 |
| BGM·효과음 | BGM 루프 재생, 블록 배치 시 효과음 재생 (`AudioManager` 싱글턴) |

---

## 아키텍처

단일 씬 기반 심플 구성. `StackManager`가 게임 루프를 총괄하고, 블록 이동은 `BlockMover`에 위임.

| 레이어 | 역할 | 주요 클래스 |
|---|---|---|
| 게임 매니저 | 입력 감지, 블록 생성·배치, 슬라이싱 판정, 점수·게임오버 관리, 속도 상한 제어 | `StackManager` |
| 블록 데이터 | 블록 크기·위치 관리, 슬라이싱 연산, Debris 생성 | `Block` |
| 블록 이동 | X/Z축 단방향 이동, 정지 처리 | `BlockMover` |
| 유틸리티 | 블록 색상 설정 | `ColorModifier` |
| UI 매니저 | 점수·최고점수 표시, 게임오버 패널 제어, 재시작 처리 | `UIManager` |
| 오디오 매니저 | BGM 루프 재생, 효과음(SFX) 재생, 싱글턴으로 전역 접근 | `AudioManager` |

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
│   │   ├── UIManager.cs               # UI 관리 (점수, 게임오버 패널, 재시작)
│   │   └── AudioManager.cs            # BGM·SFX 재생 관리 (싱글턴)
│   ├── Sounds/                        # 효과음·BGM mp3 파일
│   └── Settings/
│       ├── Mobile_RPAsset.asset       # URP 모바일 설정
│       └── PC_RPAsset.asset           # URP PC 설정
├── Packages/
└── ProjectSettings/
```

---

## 데이터 흐름

```
플레이어 입력 (클릭 / 터치 / 스페이스바)
    └─▶ StackManager.PlaceBlock()
            ├─▶ BlockMover.Stop()          — 블록 정지
            ├─▶ 겹침(overlap) 계산
            │       ├─ overlap ≤ 0         → GameOver()
            │       │                         └─▶ UIManager.ShowGameOver()
            │       ├─ |offset| < 0.1      → 퍼펙트 판정 (크기 유지 + 위치 보정)
            │       └─ 그 외              → Block.Slice() + Block.SpawnDebris()
            ├─▶ UIManager.AddScore()       — 점수·최고점수 갱신
            └─▶ SpawnNext()                — 다음 블록 생성 + 카메라 상승

경계 이탈 감지 (Update 루프)
    └─▶ IsOutBound() == true → GameOver()
```

---

## 변경 이력

| 날짜 | 내용 |
|---|---|
| 2026-04-08 | AudioManager 추가 (BGM 루프·효과음 싱글턴, AudioSource 2채널 구성) |
| 2026-04-08 | 최고 점수 PlayerPrefs 영구 저장 구현 (`SaveBestScore` / `PlayerPrefs.GetInt`) |
| 2026-04-07 | UIManager 클래스 추가 (점수·최고점수 UI, 게임오버 패널, 재시작 버튼) |
| 2026-04-06 | 난이도 속도 증가에 상한선 추가 (`minMoveSpeed`·`maxMoveSpeed` Inspector 노출), 선형 증가 계수 0.1→0.05 조정 |
| 2026-04-06 | StackManager·Block 핵심 로직 구현 (슬라이싱, 퍼펙트 판정, 점수 UI, 카메라 추적) |
| 2026-04-04 | 프로젝트 초기 세팅, BlockMover 이동·정지 구현 |

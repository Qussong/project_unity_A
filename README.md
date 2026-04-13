# ProjectA

> 하이퍼캐주얼 게임 **Stack** 모작 — Unity로 구현하는 블록 쌓기 게임

`[📷 이미지 배치 예정 - 게임 플레이 화면]`

| 항목 | 내용 |
|---|---|
| 장르 | 하이퍼캐주얼 / 퍼즐 |
| 플랫폼 | PC · Mobile |
| 엔진 | Unity 6 (URP) |
| 언어 | C# |
| 버전 | 0.6.0 |

---

## 화면 구성

| 홈 패널 | 게임 화면 | 게임오버 패널 |
|---|---|---|
| `[📷 이미지 배치 예정 - 홈/시작 화면]` | `[📷 이미지 배치 예정 - 메인 게임 플레이]` | `[📷 이미지 배치 예정 - 게임오버 패널]` |

```
[홈 패널] → (START 버튼) → [블록 왕복 이동 중] → [탭 입력]
                                                    |
                              ┌─────────────────────┴──────────────────┐
                              │ 겹침 있음                               │ 겹침 없음
                              ▼                                         ▼
                       [슬라이싱 후 다음 블록]                   [게임오버 패널]
                              ↑                                         |
                              └─────────────────────────────────────────┘
                                                           (Restart → 상태 리셋)
```

| 화면 | 설명 |
|---|---|
| 홈 패널 | 게임 시작 전 대기 화면, START 버튼으로 게임 진입 |
| 게임 플레이 | 블록 왕복 이동 → 탭으로 정지 → 겹침 판정 → 슬라이싱 반복, 점수·최고점수 실시간 표시 |
| 게임오버 패널 | 최종 점수 표시 + Restart 버튼으로 상태 리셋 재시작 |

- 최고 점수는 `PlayerPrefs`로 기기에 영구 저장됨
- Restart는 씬 재로드 없이 블록 제거 + 상태 변수 초기화로 처리

---

## 핵심 기능

| 기능 | 설명 |
|---|---|
| 블록 왕복 이동 | X축·Z축 교대로 `-moveRange`~`+moveRange` 범위를 왕복, 점수에 비례해 속도 증가 (`minMoveSpeed`~`maxMoveSpeed`) |
| 탭 입력 감지 | 마우스 클릭·모바일 터치·스페이스바 통합 처리, UI 위 클릭은 `EventSystem`으로 차단 |
| 블록 슬라이싱 | 겹친 영역만 잔류, 초과 부분은 Debris로 분리 후 중력 낙하 |
| 퍼펙트 판정 | 오차율 10% 미만 시 크기 유지 및 위치 자동 보정 |
| 판정 텍스트 | 배치 결과에 따라 GOOD·BAD·PERFECT 텍스트 애니메이션 표시 (`PixelBattleText` 라이브러리) |
| 겹침 없음 게임오버 | 탭 시 이전 블록과 겹치는 영역이 없으면 게임오버 |
| 누적 스택 | 잔류 블록 위에 다음 블록 생성, 카메라 자동 상승 (DOTween `OutCubic` 보간) |
| 색상 변화 | HSV 색상환을 따라 층마다 파스텔톤으로 블록 색상 점진 변화 (S=0.35, V=0.98) |
| 블록 탄성 | 블록 배치 시 Y 스케일 squish→spring 코루틴으로 떡 눌리는 탄성 표현 |
| Debris 탄성 | 잘린 조각에 PhysicsMaterial 적용, 바닥 충돌 시 통통 튕김 |
| 점수 UI | 현재 점수·최고 점수 실시간 표시 (TextMeshPro), 최고점수 갱신 시 자동 저장 |
| BGM·효과음 | BGM 루프 재생, 블록 배치 효과음 재생 (`SoundManager` 싱글턴, 3채널) |
| 음소거 토글 | 인게임 버튼으로 BGM·SFX 동시 뮤트, 버튼 스프라이트 ON/OFF 전환 |
| BGM 볼륨 페이드 | 화면 전환 시 BGM 볼륨을 DOTween으로 부드럽게 전환 (홈·게임오버 0.5, 게임 중 0.15) |

---

## 아키텍처

단일 씬 기반 심플 구성. `StackManager`가 게임 루프를 총괄하고, 블록 이동은 `BlockMover`에 위임.

| 레이어 | 역할 | 주요 클래스 |
|---|---|---|
| 게임 매니저 | 입력 감지, 블록 생성·배치, 슬라이싱 판정, 점수·게임오버 관리 | `StackManager` |
| 블록 데이터 | 블록 크기·위치 관리, 슬라이싱 연산, Debris 생성, 탄성 애니메이션 | `Block` |
| 블록 이동 | X/Z축 왕복 이동, 범위 끝 방향 반전, 정지 처리 | `BlockMover` |
| 유틸리티 | 블록 색상 설정 | `ColorModifier` |
| UI 매니저 | 점수·최고점수 표시, 홈·게임오버 패널 제어, 음소거 버튼 스프라이트 갱신 | `UIManager` |
| 오디오 매니저 | BGM·SFX 재생, 뮤트 토글, `OnMuteChanged` 이벤트 발행, 싱글턴 | `SoundManager` |
| 배경 캐릭터 | NavMesh 기반 참새 배회 AI | `SparrowController` |

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
│   │   ├── StackManager.cs            # 게임 루프 총괄 매니저
│   │   ├── Block.cs                   # 블록 데이터·슬라이싱 로직
│   │   ├── BlockMover.cs              # 블록 왕복 이동·정지 처리
│   │   ├── UIManager.cs               # UI 관리 (점수, 패널, 음소거 버튼)
│   │   ├── SoundManager.cs            # BGM·SFX 재생 관리 (싱글턴)
│   │   ├── SparrowController.cs       # 참새 NavMesh 배회 AI
│   │   ├── InputManager.cs            # 입력 감지 (미사용, StackManager 내장)
│   │   └── ColorModifier.cs           # 블록 색상 설정
│   ├── Sounds/                        # 효과음·BGM 파일
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
            │       │                         └─▶ UIManager.ShowGameOver(score)
            │       ├─ |offset|/blockSize < 0.1 → 퍼펙트 판정 (크기 유지 + 위치 보정)
            │       └─ 그 외              → Block.Slice() + Block.SpawnDebris()
            ├─▶ UIManager.SetScore(score)  — 점수 UI 갱신 (최고점수 갱신·저장 포함)
            └─▶ SpawnNext()                — 다음 블록 생성 + 카메라 상승 (DOTween)

음소거 버튼 클릭
    └─▶ SoundManager.ToggleMute()
            ├─▶ _asBGM.mute / _asSFX.mute 전환
            └─▶ OnMuteChanged(isMuted) → UIManager.UpdateSoundButtonSprite()

화면 전환 (StartGame / GameOver)
    └─▶ SoundManager.FadeBGMVolume(target, duration)
            ├─▶ _asBGM.DOKill()          — 진행 중인 페이드 중단
            └─▶ _asBGM.DOFade(target, duration) — 목표 볼륨으로 부드럽게 전환
```

---

## 변경 이력

| 날짜 | 내용 |
|---|---|
| 2026-04-13 | BGM 및 블록 배치 효과음 클립 교체 |
| 2026-04-13 | BGM 볼륨 페이드 추가 (`SoundManager.FadeBGMVolume`, DOTween `DOFade` + `DOKill` 중복 방지) |
| 2026-04-13 | 음소거 토글 구현 (`SoundManager.ToggleMute`, `OnMuteChanged` 이벤트, 버튼 스프라이트 전환) |
| 2026-04-13 | UI 버튼 클릭 시 블록 배치 차단 (`EventSystem.IsPointerOverGameObject`) |
| 2026-04-13 | AudioManager → SoundManager 리네임, `PlayBGM()` 함수 분리 |
| 2026-04-13 | 점수 관리 구조 변경: `UIManager.AddScore()` → `StackManager`가 점수 계산 후 `UIManager.SetScore()` 호출 |
| 2026-04-10 | 카메라 이동을 DOTween `OutCubic` 보간으로 부드럽게 변경 |
| 2026-04-10 | Restart를 씬 재로드 → `ResetGame()` 상태 리셋으로 변경 (블록 제거 + 변수 초기화) |
| 2026-04-10 | 홈 패널(시작 화면) 추가, START 버튼으로 게임 진입 |
| 2026-04-09 | 블록 배치 시 GOOD·BAD·PERFECT 판정 텍스트 애니메이션 추가 (`PixelBattleText` 라이브러리) |
| 2026-04-09 | 블록 배치 시 squish·spring 탄성 애니메이션 추가 (`Block.PlayBounceEffect`) |
| 2026-04-09 | Debris에 PhysicsMaterial 적용, 바닥 탄성 처리 (`bounciness=0.5`) |
| 2026-04-09 | 블록 색상 파스텔톤으로 변경 (HSV S=0.35, V=0.98, h 간격 0.07) |
| 2026-04-08 | 블록 왕복 이동으로 변경 (BlockMover 방향 반전), 경계 이탈 게임오버 제거 → 겹침 없음 시에만 게임오버 |
| 2026-04-08 | SoundManager 추가 (BGM 루프·효과음 싱글턴, AudioSource 3채널 구성) |
| 2026-04-08 | 최고 점수 PlayerPrefs 영구 저장 구현 |
| 2026-04-07 | UIManager 클래스 추가 (점수·최고점수 UI, 게임오버 패널, 재시작 버튼) |
| 2026-04-06 | 난이도 속도 증가에 상한선 추가 (`minMoveSpeed`·`maxMoveSpeed` Inspector 노출) |
| 2026-04-06 | StackManager·Block 핵심 로직 구현 (슬라이싱, 퍼펙트 판정, 점수 UI, 카메라 추적) |
| 2026-04-04 | 프로젝트 초기 세팅, BlockMover 이동·정지 구현 |

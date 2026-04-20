using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI Panel")]
    public GameObject panelUI;
    public TMP_Text textScore;
    public TMP_Text textBestScore;
    public Button btnSoundControl;
    public List<Sprite> listSoundControlSprite; // 0 : ON , 1 : OFF

    [Header("Home Panel")]
    public GameObject panelHome;
    public Button btnStartGame;

    [Header("InGame Panel")]
    public GameObject panelInGame;
    // public Button btnPlaceBlock;
    public TouchHandler touchHandlerPlaceBlock;

    [Header("GameOver Panel")]
    public GameObject panelGameOver;
    public TMP_Text textFinalScore;
    public Button btnRestart;

    private int _bestScore = 0;

    private void Awake()
    {
        // UI 패널 표출
        panelUI.SetActive(true);

        // 홈 패널 표출
        panelHome.SetActive(true);

        // 인게임 패널 숨김
        panelInGame.SetActive(false);

        // 게임오버 패널 숨기기
        panelGameOver.SetActive(false);
    }

    void Start()
    {
        // 저장된 최고 점수 불러오기
        _bestScore = PlayerPrefs.GetInt("BestScore", 0);

        UpdateBestScoreUI();

        // 음소거 버튼 리스너 등록
        btnSoundControl.onClick.AddListener(SoundManager.Instance.ToggleMute);
        btnSoundControl.onClick.AddListener(() => HapticManager.Vibrate(HapticManager.HapticType.Wiggle));

        // SoundManager.OnMuteChanged 함수 등록
        SoundManager.Instance.OnMuteChanged += UpdateSoundButtonSprite;
    }

    private void UpdateSoundButtonSprite(bool isMute)
    {
        if(isMute)
        {
            btnSoundControl.GetComponent<Image>().sprite = listSoundControlSprite[1];
        }
        else
        {
            btnSoundControl.GetComponent<Image>().sprite = listSoundControlSprite[0];
        }
    }

    // 점수 추가
    public void SetScore(int score)
    {
        // _currentScore += amount;
        textScore.text = score.ToString();

        // 최고 점수 갱신
        if(score > _bestScore)
        {
            _bestScore = score;
            UpdateBestScoreUI();    // 최고점수 UI 표시 갱신
            SaveBestScore();        // 최고점수 저장
        }
    }

    // 게임오버 패널 활성화
    public void ShowGameOver(int score)
    {
        textFinalScore.text = score.ToString();
        panelGameOver.SetActive(true);
    }

    // 최고점수 UI 갱신
    private void UpdateBestScoreUI()
    {
        textBestScore.text = _bestScore.ToString();
    }

    private void ReStartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void SaveBestScore()
    {
        PlayerPrefs.SetInt("BestScore", _bestScore);
    }

}

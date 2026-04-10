using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("시작화면 UI")]
    public GameObject homePanel;
    public Button btnStartGame;

    [Header("인게임 UI")]
    public TMP_Text textScore;
    public TMP_Text textBestScore;

    [Header("게임오버 패널")]
    public GameObject gameOverPanel;
    public TMP_Text textFinalScore;
    public Button btnRestart;

    private int _currentScore = 0;
    private int _bestScore = 0;

    void Start()
    {
        // 저장된 최고 점수 불러오기
        _bestScore = PlayerPrefs.GetInt("BestScore", 0);
        
        UpdateBestScoreUI();

        // 홈 화면 패널 표출
        homePanel.SetActive(true);

        // 게임오버 패널 숨기기
        gameOverPanel.SetActive(false);
    }

    // 점수 추가
    public void AddScore(int amount = 1)
    {
        _currentScore += amount;
        textScore.text = _currentScore.ToString();

        // 최고 점수 갱신
        if(_currentScore > _bestScore)
        {
            _bestScore = _currentScore;
            UpdateBestScoreUI();    // 최고점수 UI 표시 갱신
            SaveBestScore();    // 최고점수 저장
        }
    }

    // 게임오버 패널 활성화
    public void ShowGameOver()
    {
        textFinalScore.text = _currentScore.ToString();
        gameOverPanel.SetActive(true);
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

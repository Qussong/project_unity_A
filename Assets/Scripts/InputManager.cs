using UnityEngine;

public class InputManager : MonoBehaviour
{
    private BlockMover _currentBlock;    // 현재 움직이는 블록

    void Start()
    {
        // 씬에 있는 첫 번째 블록 찾기
        _currentBlock = FindAnyObjectByType<BlockMover>();
    }

    void Update()
    {
        // 마우스 클릭 또는 모바일 터치 둘 다 감지
        bool isMouseTouched = Input.GetMouseButtonDown(0);
        bool isMobileTouched = Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;
        bool bTapped = isMouseTouched || isMobileTouched;

        if (bTapped && _currentBlock != null)
        {
            _currentBlock.Stop();
            _currentBlock = null;   // 다음 탭을 위해 비워줌
        }

    }
}

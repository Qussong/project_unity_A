using UnityEngine;

public class Block : MonoBehaviour
{
    // 현재 블록의 크기와 위치 (외부에서 읽기용)
    public float Width => transform.localScale.x;
    public float Length => transform.localScale.z;
    public float PosX => transform.position.x;
    public float PosZ => transform.position.z;

    // private float _debrisSize = -1;

    // 블록 초기화 - StackManager에서 호출
    public void Init(Vector3 pos, Vector3 scale, Color color)
    {
        transform.position = pos;
        transform.localScale = scale;
        GetComponent<Renderer>().material.color = color;
    }

    // 잘라내기 - 겹침 영역만 남기고 초과 조각을 떨어뜨림
    // 반환값 : 잘라낸 후 남은 블록 크기 (다음 블록의 크기로 쓰임)
    public void Slice(float overlap, float newCenter, bool onX)
    {
        // 조각의 크기
        // _debrisSize = onX ? Width - overlap : Length - overlap;

        // 잘려나가는 조각 생성 (크기·위치 변경 전에 호출해야 원본 값 사용 가능)
        // SpawnDebris(debrisSize, newCenter, onX);

        // 겹침 영역에 맞게 블록 크기, 위치 조정
        if (onX)
        {
            transform.localScale = new Vector3(overlap,
                                                transform.localScale.y,
                                                transform.localScale.z);
            transform.position = new Vector3(newCenter,
                                                transform.position.y,
                                                transform.position.z);
        }
        else
        {
            transform.localScale = new Vector3(transform.localScale.x,
                                                transform.localScale.y,
                                                overlap);
            transform.position = new Vector3(transform.position.x,
                                                transform.position.y,
                                                newCenter);
        }
    }

    public void SpawnDebris(float debrisSize, float lastCenter, float newCenter, bool onX)
    {
        // 조각 생성
        GameObject debris = GameObject.CreatePrimitive(PrimitiveType.Cube);
        debris.name = "Debris";

        // 조각 크기 설정
        Vector3 debrisScale = transform.localScale;
        if (onX)
            debrisScale.x = debrisSize;
        else
            debrisScale.z = debrisSize;
        debris.transform.localScale = debrisScale;

        // 조각 위치 계산
        // 남은 블록 중심(newCenter)에서 (남은크기/2 + 조각크기/2) 만큼 바깥으로
        Vector3 debrisPos = transform.position;
        float sign = newCenter > lastCenter ? 1f : -1f;
        if (onX)
            debrisPos.x = newCenter + sign * (Width / 2f + debrisSize / 2f);
        else
            debrisPos.z = newCenter + sign * (Length / 2f + debrisSize / 2f);

        // 조각 위치 설정
        debris.transform.position = debrisPos;

        // 색상
        debris.GetComponent<Renderer>().material.color = GetComponent<Renderer>().material.color;

        // 중력으로 떨어짐
        Rigidbody rb = debris.AddComponent<Rigidbody>();
        rb.AddForce(new Vector3(0, -2f, 0), ForceMode.Impulse);

        Destroy(debris, 3f);
    }
}

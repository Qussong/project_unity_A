using UnityEngine;

public class Block : MonoBehaviour
{
    // 현재 블록의 크기와 위치 (외부에서 읽기용)
    public float Width => transform.localScale.x;
    public float Depth => transform.localScale.z;
    public float PosX => transform.position.x;
    public float PosZ => transform.position.z;

    // 블록 초기화 - StackManager에서 호출
    public void Init(Vector3 pos, Vector3 scale, Color color)
    {
        transform.position = pos;
        transform.localScale = scale;
        GetComponent<Renderer>().material.color = color;
    }

    // 잘라내기 - 겹침 영역만 남기고 초과 조각을 떨어뜨림
    // 반환값 : 잘라낸 후 남은 블록 크기 (다음 블록의 크기로 쓰임)
    public float Slice(float overlap, float newCenter, bool onX)
    {
        float overSize = onX ? Width - overlap : Depth - overlap; // 잘려나갈 조각의 크기

        // 남는 블록 위치, 크기 조정
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


        // 잘려나가는 조각 생성
        SpawnDebris(overSize, newCenter, onX);

        return overlap; // 남은 크기 반환 (다음 블록이 이 크기로 생성됨)
    }

    private void SpawnDebris(float debrisSize, float newCenter, bool onX)
    {
        GameObject debris = GameObject.CreatePrimitive(PrimitiveType.Cube);
        debris.name = "Debris";

        // 조각 크기
        Vector3 debrisScale = transform.localScale;
        if (onX)
            debrisScale.x = debrisSize;
        else
            debrisScale.z = debrisSize;
        debris.transform.localScale = debrisScale;

        // 조각 위치 계산
        // 남은 블록 중심(newCenter)에서 (남은크기/2 + 조각크기/2) 만큼 바깥으로
        Vector3 debrisPos = transform.position;

        if (onX)
        {
            float remainHalf = debrisScale.x / 2f;
            float debrisHalf = debrisSize / 2f;

            // newCenter가 원래 위치보다 오른쪽 -> 조각은 외쪽
            // newCenter가 원래 위치보다 왼쪽 -> 조각은 오른쪽
            float sign = newCenter > transform.position.x ? -1f : 1f;
            debrisPos.x = newCenter + sign * (remainHalf + debrisHalf);
        }
        else
        {
            float remainHalf = debrisScale.z / 2f;
            float debrisHalf = debrisSize / 2f;

            float sign = newCenter > transform.position.z ? -1f : 1f;
            debrisPos.z = newCenter + sign * (remainHalf + debrisHalf);
        }

        debris.transform.position = debrisPos;

        // 색상
        debris.GetComponent<Renderer>().material.color = GetComponent<Renderer>().material.color;

        // 중력으로 떨어짐
        Rigidbody rb = debris.AddComponent<Rigidbody>();
        rb.AddForce(new Vector3(UnityEngine.Random.Range(-1f, 1f), -2f,
                                UnityEngine.Random.Range(-1f, 1f)), ForceMode.Impulse);

        Destroy(debris, 3f);
    }
}

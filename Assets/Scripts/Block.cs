using System.Collections;
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
        // GetComponent<Renderer>().material.color = color;
        GetComponent<Renderer>().material.SetColor("_BaseColor", color);
    }

    // 잘라내기 - 겹침 영역만 남기고 초과 조각을 떨어뜨림
    // 반환값 : 잘라낸 후 남은 블록 크기 (다음 블록의 크기로 쓰임)
    public void Slice(float overlap, float newCenter, bool onX)
    {
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

        // 색상 - 블록의 머티리얼을 복사해서 적용 (CreatePrimitive의 기본 셰이더는 URP 빌드에서 마젠타가 됨)
        Renderer blockRenderer = GetComponent<Renderer>();
        Renderer debrisRenderer = debris.GetComponent<Renderer>();
        debrisRenderer.material = new Material(blockRenderer.material);

        // 블록의 매시를 Debris에 복사
        debris.GetComponent<MeshFilter>().mesh = GetComponent<MeshFilter>().mesh;

        // 중력으로 떨어짐
        Rigidbody rb = debris.AddComponent<Rigidbody>();
        rb.AddForce(new Vector3(0, -2f, 0), ForceMode.Impulse);

        // 조각에 탄성 부여
        PhysicsMaterial bounceMat = new PhysicsMaterial();
        bounceMat.bounciness = 0.5f;
        bounceMat.dynamicFriction = 0.2f;
        bounceMat.staticFriction = 0.2f;
        bounceMat.bounceCombine = PhysicsMaterialCombine.Maximum;
        debris.GetComponent<Collider>().material = bounceMat;

        // 3초 후 파괴
        Destroy(debris, 3f);
    }

    // 떡 효과
    public void PlayBounceEffect()
    {
        StartCoroutine(BounceCoroutine());
    }

    private IEnumerator BounceCoroutine()
    {
        float origScaleY = transform.localScale.y;
        float origPosY   = transform.localPosition.y;

        // (목표 상대 스케일, 지속 시간) 키프레임
        float[] scales    = { 0.60f, 1.15f, 0.92f, 1.05f, 0.99f, 1.00f };
        float[] durations = { 0.08f, 0.14f, 0.10f, 0.08f, 0.05f, 0.03f };

        float prev = 1.0f;
        for (int i = 0; i < scales.Length; i++)
        {
            float elapsed = 0f;
            float start = prev;
            while (elapsed < durations[i])
            {
                elapsed += Time.deltaTime;
                float rel = Mathf.Lerp(start, scales[i], elapsed / durations[i]);

                Vector3 s = transform.localScale;
                s.y = origScaleY * rel;
                transform.localScale = s;

                // 바닥 위치 고정 (위로만 튕김)
                Vector3 p = transform.localPosition;
                p.y = origPosY - (origScaleY - s.y) / 2f;
                transform.localPosition = p;

                yield return null;
            }
            prev = scales[i];
        }

        // 정확히 원래 값으로 복원
        Vector3 fs = transform.localScale; fs.y = origScaleY; transform.localScale = fs;
        Vector3 fp = transform.localPosition; fp.y = origPosY; transform.localPosition = fp;
    }
}

using UnityEngine;

public class ColorModifier : MonoBehaviour
{
    public Color color;

    void Start()
    {
        GetComponent<Renderer>().material.color = color;
    }
}

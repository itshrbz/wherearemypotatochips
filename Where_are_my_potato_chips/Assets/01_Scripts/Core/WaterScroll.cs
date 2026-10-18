using UnityEngine;

public class WaterScroll : MonoBehaviour
{
    public Vector2 speed = new Vector2(0.02f, 0.01f);
    Renderer rend;
    Material mat;
    Vector2 offset;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        mat = rend.material;   // copia propia: no modifica el material del proyecto
    }

    void Update()
    {
        offset += speed * Time.deltaTime;
        offset.x %= 1f; offset.y %= 1f;
        if (mat.HasProperty("_BaseMap")) mat.SetTextureOffset("_BaseMap", offset);
        if (mat.HasProperty("_BumpMap")) mat.SetTextureOffset("_BumpMap", -offset * 0.7f);
    }
}
using UnityEngine;

// Anima un SpriteRenderer cambiando de sprite (fuego, laseres, etc.). No necesita Animator.
public class SpriteAnimador : MonoBehaviour
{
    public Sprite[] cuadros;
    public float fps = 8f;
    public bool aleatorizarInicio = true;

    SpriteRenderer sr;
    float t;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (aleatorizarInicio) t = Random.value * 3f;
    }

    void Update()
    {
        if (sr == null || cuadros == null || cuadros.Length == 0) return;
        t += Time.deltaTime;
        sr.sprite = cuadros[(int)(t * fps) % cuadros.Length];
    }
}

using UnityEngine;

[RequireComponent(typeof(Animation))]
public class AnimationStartOffset : MonoBehaviour
{
    public bool m_RandomStart;
    [Range(0.0f, 1.0f)]public float m_StartOffset;

    private void Awake()
    {
        Animation l_animation = GetComponent<Animation>();
        l_animation[l_animation.clip.name].normalizedTime = m_RandomStart ? Random.value : m_StartOffset;
        l_animation.Sample();
    }
}

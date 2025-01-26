using UnityEngine;

public class BoostProperties : MonoBehaviour
{
    [SerializeField] [Tooltip("Multiplier to use against thrust speed")]
    public float m_BoostMultiplier = 3f;
    
    [SerializeField] [Tooltip("Default duration of a boost")]
    public float m_BoostDuration = 2f;
    
    [SerializeField] [Tooltip("Degrees per second to rotate while boosting")]
    public float m_BoostRotation = 2160f;
}

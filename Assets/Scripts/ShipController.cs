using System.Collections;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent (typeof (Rigidbody))]
public class ShipController : MonoBehaviour
{
    [SerializeField] [Tooltip("UI Element label")]
    public TMP_Text m_ShipPositionLabel;

    [SerializeField] [Tooltip("Audio source for background music player")]
    public AudioSource m_BGAudioSource;
    
    [SerializeField] [Tooltip("Animated train")]
    public GameObject m_AnimatedTrain;
    
    [SerializeField] [Tooltip("Animated train movement speed")]
    public float m_AnimatedTrainSpeed = 1.5f;

    [SerializeField] [Tooltip("Elapsed seconds until the starting intro is done")]
    public float m_StartIntroTime = 6f;
    
    [Header("Movement")]
    
    [SerializeField] [Tooltip("Base speed ship moves forward by")]
    public float m_ThrustSpeed = 1f;

    [SerializeField] [Tooltip("Base speed ship rolls left/right by")]
    public float m_RollSpeed = 1f;

    [SerializeField] [Tooltip("Base speed ship tilts up/down by")]
    public float m_PitchSpeed = 1f;

    [SerializeField] [Tooltip("The min/max x bounds of the ship")]
    public Vector2 m_XBounds;

    [SerializeField] [Tooltip("The min/max y bounds of the ship")]
    public Vector2 m_YBounds;
    
    private Rigidbody m_Rigidbody;
    private Vector2 m_MoveVector = Vector2.zero;

    private float m_StartingThrustSpeed = 0f;
    private bool b_IsBoosting = false;
    private BoostProperties m_BoostProperties = null;
    private int m_OxygenLevel = 0;
    private Coroutine m_BoostCoRoutine;
    private bool b_IsTrainMoving = false;

    private void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Rigidbody.useGravity = false;
        m_Rigidbody.isKinematic = false;

        m_StartingThrustSpeed = m_ThrustSpeed;
        m_ThrustSpeed = 3f;
        
        StartCoroutine(TriggerAnimatedStart());
    }

    private void Update()
    {
        m_ThrustSpeed = Mathf.Lerp(0, m_StartingThrustSpeed, Time.time / m_StartIntroTime);

        if (m_BGAudioSource)
        {
            m_BGAudioSource.volume = Mathf.Lerp(0, 0.8f, Time.time / m_StartIntroTime);
        }

        if (b_IsBoosting && m_BoostProperties is not null)
        {
            UpdateBoostedMovement();
        }
        else
        {
            UpdateControlledMovement();
        }
        
        m_ShipPositionLabel.text = $"Oxygen Level: {Mathf.Min(m_OxygenLevel, 100)}% | Game time: {Time.time}";

        if (m_Rigidbody.position.z >= 520)
        {
            SceneManager.LoadScene("GameOver");
        }

        if (b_IsTrainMoving && m_AnimatedTrain)
        {
            m_AnimatedTrain.transform.position += new Vector3(m_AnimatedTrainSpeed, 0f, 0f);
        }
    }

    private void UpdateControlledMovement()
    {
        bool canControl = Time.time >= m_StartIntroTime;
        
        var prevPosition = m_Rigidbody.position;
        
        var nextMovement = new Vector3(
            canControl ? m_MoveVector.x : 0, 
            canControl ? -m_MoveVector.y : 0, 
            1
        ).normalized;
        
        var nextPosition = m_Rigidbody.position +
                           new Vector3(
                               nextMovement.x * m_RollSpeed, 
                               nextMovement.y * m_PitchSpeed,
                               nextMovement.z * m_ThrustSpeed
                            )
                           * Time.deltaTime;

        // Limit position to +/- 6 on x and 0-3 on y
        nextPosition.x = Mathf.Clamp(nextPosition.x, m_XBounds.x, m_XBounds.y);
        nextPosition.y = Mathf.Clamp(nextPosition.y, m_YBounds.x, m_YBounds.y);
        
        // Move based on user input and limits
        m_Rigidbody.MovePosition(nextPosition);
        m_Rigidbody.linearVelocity = Vector3.zero;

        // Rotate based on user input
        var hasMovedHorizontally = !Mathf.Approximately(prevPosition.x, nextPosition.x);
        var hasMovedVertically = !Mathf.Approximately(prevPosition.y, nextPosition.y);

        m_Rigidbody.rotation = Quaternion.Euler(new Vector3(
            hasMovedVertically ? -30f * nextMovement.y : 0, 
            0f, 
            hasMovedHorizontally ? -30f * nextMovement.x : 0
        ));
    }

    private void UpdateBoostedMovement()
    {
        // Move forward based on boost multiplier
        var nextPosition = m_Rigidbody.position + new Vector3(
            0, 
            0, 
            1 * m_ThrustSpeed * m_BoostProperties.m_BoostMultiplier * Time.deltaTime
        );
        
        m_Rigidbody.MovePosition(nextPosition);
        m_Rigidbody.linearVelocity = Vector3.zero;
        
        // Rotate constantly, only on the z-axis, if needed
        // m_Rigidbody.rotation = Quaternion.Euler(new Vector3(
        //     0f, 
        //     0f, 
        //     m_Rigidbody.rotation.z
        // ));
        m_Rigidbody.transform.Rotate(0, 0, m_BoostProperties.m_BoostRotation * Time.deltaTime);
    }
    
    private IEnumerator TriggerBoost([CanBeNull] BoostProperties properties)
    {
        if (b_IsBoosting || properties is null) yield break;

        m_BoostProperties = properties;
        b_IsBoosting = true;
        yield return new WaitForSeconds(m_BoostProperties.m_BoostDuration);
        b_IsBoosting = false;
    }

    private IEnumerator TriggerAnimatedStart()
    {
        yield return new WaitForSeconds(m_StartIntroTime);

        b_IsTrainMoving = true;
    }

    private void PickupOxygen(GameObject oxygenBubbleObj)
    {
        var oxygenBubble = oxygenBubbleObj.GetComponent<OxygenBubble>();
        if (oxygenBubble is null)
        {
            Destroy(oxygenBubbleObj);
            return;
        }

        m_OxygenLevel += oxygenBubble.m_Amount;
        Destroy(oxygenBubbleObj);
    }

    private void HandleEndGame()
    {
        if (m_OxygenLevel >= 100)
        {
            SceneManager.LoadScene("EndGame");
            return;
        }
        
        SceneManager.LoadScene("GameOver");
    }

    public void OnMove(InputValue value)
    {
        m_MoveVector = value.Get<Vector2>();
    }

    public void OnTriggerEnter(Collider triggerCollider)
    {
        switch (triggerCollider.gameObject.tag)
        {
            case "Boost":
                var properties = triggerCollider.gameObject.GetComponentInParent<BoostProperties>();

                if (m_BoostCoRoutine is not null)
                {
                    StopCoroutine(m_BoostCoRoutine);
                    b_IsBoosting = false;
                }
                
                m_BoostCoRoutine = StartCoroutine(TriggerBoost(properties));
                break;
            case "Oxygen":
                PickupOxygen(triggerCollider.gameObject);
                break;
            case "Endgame":
                HandleEndGame();
                break;
            default:
                Debug.Log($"Unhandled OnTriggerEnter; Tag: {triggerCollider.gameObject.tag} | Name: {triggerCollider.gameObject.name}");
                break;
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"OnCollisionEnter; Tag: {collision.gameObject.tag} | Name: {collision.gameObject.name}");
    }
}

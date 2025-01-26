using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour
{
    [SerializeField] [Tooltip("Will start this scene when any input is pressed")]
    public string m_NextScene;

    void Start()
    {
        InputSystem.onAnyButtonPress.CallOnce(currentAction =>
        {
            if (m_NextScene is null) return;
            SceneManager.LoadScene(m_NextScene);
        });
    }
}

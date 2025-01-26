using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// An add-on module for Cinemachine Virtual Camera that locks the camera's Z co-ordinate
/// </summary>
[ExecuteInEditMode] [SaveDuringPlay] [AddComponentMenu("")] // Hide in menu
public class LockCameraAxis : CinemachineExtension
{
    [Header("Locked positions")]
    public float m_YPosition = 6;
    public float m_XPosition = 0;

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if (stage != CinemachineCore.Stage.Body) return;
        
        var pos = state.RawPosition;
        pos.x = m_XPosition;
        pos.y = m_YPosition;
            
        state.RawPosition = pos;
    }
}
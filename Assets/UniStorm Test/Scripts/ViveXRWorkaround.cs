// Reference: https://discussions.unity.com/t/rendering-issue-with-htc-vive-focus-openxr-and-unity-6/1543857
// From: Vigo
//////////////////////////////////////////////////////////////////////////
//USING
//////////////////////////////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
//////////////////////////////////////////////////////////////////////////
//NAMESPACE
//////////////////////////////////////////////////////////////////////////
namespace Diing.VIVE
{
    //////////////////////////////////////////////////////////////////////////
    //CLASS
    //////////////////////////////////////////////////////////////////////////
    [AddComponentMenu("Diing/Vive/XR Workaround")]
    public class ViveXRWorkaround  : MonoBehaviour
    {
        public static bool IsVive =>
            SystemInfo.deviceModel.Contains("Vive", System.StringComparison.OrdinalIgnoreCase) ||
            SystemInfo.deviceName.Contains("Vive", System.StringComparison.OrdinalIgnoreCase);
        //////////////////////////////////////////////////////////////////////////
        //FIELD
        //////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////
        //FUNCTION
        //////////////////////////////////////////////////////////////////////////
        private void OnEnable()
        {
            if (IsVive)
                StartCoroutine(RemoveOcclusionMask());
            else
                Destroy(this);
        }
        //////////////////////////////////////////////////////////////////////////
        IEnumerator RemoveOcclusionMask()
        {
            // Find DisplaySubsystem

            XRDisplaySubsystem display = null;

            List<XRDisplaySubsystem> displaySubsystems = new List<XRDisplaySubsystem>();

            do
            {
                SubsystemManager.GetSubsystems(displaySubsystems);

                foreach (var d in displaySubsystems)
                {
                    if (d.running)
                    {
                        display = d;
                        break;
                    }
                }
                yield return null;
            } while (display == null);

            Debug.Log("RemoveOcclusionMask XRSettings.occlusionMaskScale = 0");
            XRSettings.occlusionMaskScale = 0;
            XRSettings.useOcclusionMesh = false;
        }
    }
    //////////////////////////////////////////////////////////////////////////
    //EVENT HANDLER
    //////////////////////////////////////////////////////////////////////////
}
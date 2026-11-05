using UnityEngine;
namespace EuroHaul.Platform
{
    public sealed class InputPlatformSwitch : MonoBehaviour
    {
        [SerializeField] private Behaviour keyboardInput;
        [SerializeField] private Behaviour touchInput;
        [SerializeField] private Canvas mobileCanvas;
        private void Awake()
        {
            bool mobile = Application.isMobilePlatform;
            if (keyboardInput != null) keyboardInput.enabled = !mobile;
            if (touchInput != null) touchInput.enabled = mobile;
            if (mobileCanvas != null) mobileCanvas.enabled = mobile;
        }
    }
}
using UnityEngine;

namespace Ssalddel.Unity.Samples.ResidentialPickup
{
    public sealed class ResidentialPickupRoleSwitchView : MonoBehaviour
    {
        [SerializeField]
        private string roleCode = string.Empty;

        [SerializeField]
        private ResidentialPickupSceneController controller = null!;

        public void Configure(string targetRoleCode, ResidentialPickupSceneController sceneController)
        {
            roleCode = targetRoleCode;
            controller = sceneController;
        }

        private async void OnMouseDown()
        {
            await controller.SwitchRoleAsync(roleCode);
        }

        public bool ValidateWiring()
        {
            return !string.IsNullOrWhiteSpace(roleCode) && controller != null;
        }
    }
}

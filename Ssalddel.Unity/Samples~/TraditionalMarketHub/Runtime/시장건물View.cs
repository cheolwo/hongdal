using Ssalddel.Unity.TraditionalMarkets;
using UnityEngine;

namespace Ssalddel.Unity.Samples.TraditionalMarketHub
{
    public sealed class 시장건물View : MonoBehaviour
    {
        [SerializeField]
        private GameObject visualRoot = null!;

        [SerializeField]
        private TextMesh marketNameText = null!;

        [SerializeField]
        private TextMesh regionText = null!;

        public void Configure(GameObject root, TextMesh nameText, TextMesh locationText)
        {
            visualRoot = root;
            marketNameText = nameText;
            regionText = locationText;
        }

        public void Render(전통시장물류거점ScreenModel model)
        {
            marketNameText.text = model.시장명;
            regionText.text = model.시도 + " " + model.시군구;
            visualRoot.SetActive(true);
        }

        public void Hide()
        {
            visualRoot.SetActive(false);
        }

        public bool ValidateWiring()
        {
            return visualRoot != null && marketNameText != null && regionText != null;
        }
    }
}

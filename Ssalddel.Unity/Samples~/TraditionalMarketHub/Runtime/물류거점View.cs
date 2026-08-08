using System;
using Ssalddel.Unity.TraditionalMarkets;
using UnityEngine;

namespace Ssalddel.Unity.Samples.TraditionalMarketHub
{
    public sealed class 물류거점View : MonoBehaviour
    {
        [SerializeField]
        private GameObject visualRoot = null!;

        [SerializeField]
        private Renderer statusRenderer = null!;

        [SerializeField]
        private TextMesh statusText = null!;

        [SerializeField]
        private TextMesh capabilityText = null!;

        [SerializeField]
        private TextMesh sourceText = null!;

        [SerializeField]
        private InteractionSocket interactionSocket = null!;

        private Action? selected;

        public void Configure(
            GameObject root,
            Renderer renderer,
            TextMesh stateText,
            TextMesh functionsText,
            TextMesh provenanceText,
            InteractionSocket socket)
        {
            visualRoot = root;
            statusRenderer = renderer;
            statusText = stateText;
            capabilityText = functionsText;
            sourceText = provenanceText;
            interactionSocket = socket;
        }

        public void Render(전통시장물류거점ScreenModel model, Action onSelected)
        {
            selected = onSelected;
            interactionSocket.Selected -= HandleSelected;
            interactionSocket.Selected += HandleSelected;

            statusText.text = "물류거점 " + model.상태Code
                + "\n일일 " + model.일일공동구매처리용량 + "건";
            capabilityText.text = BuildCapabilityLabel(model.물류기능);
            sourceText.text = model.SourceTypeCode
                + "\n" + model.SourceName
                + "\n" + model.EvidenceAsOf.ToString("yyyy-MM-dd HH:mm zzz");
            statusRenderer.material.color = model.상태Code == 전통시장물류거점상태Codes.Active
                ? new Color(0.18f, 0.62f, 0.34f)
                : new Color(0.92f, 0.62f, 0.16f);
            visualRoot.SetActive(true);
        }

        public void Hide()
        {
            selected = null;
            visualRoot.SetActive(false);
        }

        public bool ValidateWiring()
        {
            return visualRoot != null
                && statusRenderer != null
                && statusText != null
                && capabilityText != null
                && sourceText != null
                && interactionSocket != null
                && interactionSocket.ValidateWiring();
        }

        private void HandleSelected()
        {
            selected?.Invoke();
        }

        private static string BuildCapabilityLabel(전통시장물류기능ScreenModel capability)
        {
            var value = string.Empty;
            Append(ref value, capability.대량입고지원, "대량입고");
            Append(ref value, capability.분류지원, "분류");
            Append(ref value, capability.주민픽업지원, "주민픽업");
            Append(ref value, capability.마지막구간배송지원, "마지막구간배송");
            Append(ref value, capability.냉장보관지원, "냉장");
            Append(ref value, capability.냉동보관지원, "냉동");
            return string.IsNullOrEmpty(value) ? "지원 기능 없음" : value;
        }

        private static void Append(ref string value, bool enabled, string label)
        {
            if (!enabled)
            {
                return;
            }

            value = string.IsNullOrEmpty(value) ? label : value + " / " + label;
        }
    }
}

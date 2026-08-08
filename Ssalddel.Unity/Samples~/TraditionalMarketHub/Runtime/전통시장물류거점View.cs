using Ssalddel.Unity.TraditionalMarkets;
using UnityEngine;

namespace Ssalddel.Unity.Samples.TraditionalMarketHub
{
    public sealed class 전통시장물류거점View : MonoBehaviour
    {
        [SerializeField]
        private 시장건물View marketBuilding = null!;

        [SerializeField]
        private 물류거점View logisticsHub = null!;

        [SerializeField]
        private GameObject informationPanelRoot = null!;

        [SerializeField]
        private TextMesh informationText = null!;

        [SerializeField]
        private GameObject detailPanelRoot = null!;

        [SerializeField]
        private TextMesh detailText = null!;

        private 전통시장물류거점ScreenModel? current;

        public void Configure(
            시장건물View building,
            물류거점View hub,
            GameObject statusRoot,
            TextMesh statusText,
            GameObject panelRoot,
            TextMesh panelText)
        {
            marketBuilding = building;
            logisticsHub = hub;
            informationPanelRoot = statusRoot;
            informationText = statusText;
            detailPanelRoot = panelRoot;
            detailText = panelText;
        }

        public void ShowLoading()
        {
            current = null;
            informationText.text = "전통시장 물류거점 Loading...";
            informationPanelRoot.SetActive(true);
            detailPanelRoot.SetActive(false);
            marketBuilding.Hide();
            logisticsHub.Hide();
        }

        public void ShowError(string message)
        {
            current = null;
            informationText.text = "초기 조회 실패\n" + message;
            informationPanelRoot.SetActive(true);
            detailPanelRoot.SetActive(false);
            marketBuilding.Hide();
            logisticsHub.Hide();
        }

        public void Render(전통시장물류거점ScreenModel model)
        {
            current = model;
            marketBuilding.Render(model);
            logisticsHub.Render(model, OpenDetail);
            informationText.text = model.SourceTypeCode
                + " | " + model.LocationPrecisionCode
                + "\nRevision " + model.Revision;
            informationPanelRoot.SetActive(true);
            detailPanelRoot.SetActive(false);
        }

        public void OpenDetail()
        {
            if (current == null)
            {
                return;
            }

            detailText.text = current.시장명
                + "\n상태 " + current.상태Code
                + "\n서비스 반경 " + current.서비스반경Km.ToString("0.##") + "km"
                + "\n입고 " + current.입고시간대
                + " / 픽업 " + current.픽업시간대
                + "\n위치 정밀도 " + current.LocationPrecisionCode
                + "\n" + current.SourceName;
            detailPanelRoot.SetActive(true);
        }

        public bool ValidateWiring()
        {
            return marketBuilding != null
                && logisticsHub != null
                && informationPanelRoot != null
                && informationText != null
                && detailPanelRoot != null
                && detailText != null
                && marketBuilding.ValidateWiring()
                && logisticsHub.ValidateWiring();
        }
    }
}

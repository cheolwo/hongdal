window.ssalddelAddress = {
    openKakaoPostcode: function () {
        return new Promise(function (resolve, reject) {
            if (!window.kakao || !window.kakao.Postcode) {
                reject("Kakao Postcode script is not loaded.");
                return;
            }

            new kakao.Postcode({
                oncomplete: function (data) {
                    resolve({
                        zonecode: data.zonecode || "",
                        address: data.address || "",
                        roadAddress: data.roadAddress || "",
                        jibunAddress: data.jibunAddress || "",
                        userSelectedType: data.userSelectedType || "",
                        addressType: data.addressType || "",
                        sido: data.sido || "",
                        sigungu: data.sigungu || "",
                        sigunguCode: data.sigunguCode || "",
                        bcode: data.bcode || "",
                        bname: data.bname || "",
                        hname: data.hname || "",
                        roadname: data.roadname || "",
                        roadnameCode: data.roadnameCode || "",
                        buildingCode: data.buildingCode || "",
                        buildingName: data.buildingName || "",
                        apartment: data.apartment || "",
                        query: data.query || "",
                        rawJson: JSON.stringify(data)
                    });
                },
                onclose: function (state) {
                    if (state === "FORCE_CLOSE") {
                        resolve(null);
                    }
                }
            }).open({
                popupTitle: "살뜰 주소 검색",
                popupKey: "ssalddelAddressPopup"
            });
        });
    }
};

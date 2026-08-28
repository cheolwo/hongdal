# 약초·Recipe·조합 제작 문답

## 식별

- 문답 고유 식별자: `inquiry:herbal-recipe-crafting.r1`
- 대상 PlayableLoop: `playable-loop:nature-basic-herbal-recovery.v1`
- 이관 질문: `Q-045~Q-050`, `Q-061~Q-064`, `Q-068~Q-071`
- 상세 원문·조사 계보: [동결 통합 아카이브](../nature-night-day2.inquiry.r1.md)
- 상태: `Refining`

## 이 문서가 소유하는 질문

- 위험 수면 뒤 체온 안정과 약초 예방·치료
- Recipe 발견·학습·Multiplayer 전수
- 건강·위험 누적·초기 질병·심한 질병
- 미학습 Recipe 추론과 자동 관찰 카드
- 정체불명 혼합물, 재료 소비, 집중력 감소와 위협 상승
- 통합 카드 서랍의 Recipe 탭
- 3×3 자유 추론과 5×5·7×7·9×9 정식 Recipe 요구
- 플레이어 자유 Recipe 작성 보류

## 현재 확정 기준

- Solo와 Multiplayer 모두 3×3 조합을 기억·추론할 수 있다.
- Recipe 카드는 제작 허가가 아니라 정보 관리와 빠른 재사용을 돕지만, 5×5 이상 고등 조합에는 정식 Recipe 지식이 필요하다.
- 추론 성공은 `SelfExperiment` 관찰 카드를 만들고 효능·부작용은 사용·분석으로 채운다.
- 실패는 재료를 소비해 정체불명 혼합물을 만들고 집중력을 낮추며 위협을 높인다.
- Recipe·청사진·타로·역할/수집은 하나의 카드 서랍 UI를 공유하되 권위와 효과를 분리한다.
- 플레이어 자유 Recipe 작성·효과 정의·공유는 후속 revision까지 보류한다.
- 첫 따뜻한 약초차는 체온 회복·질병 위험 감소·초기 질병 치료를 제공하고 심한 질병에는 보조 효과만 제공한다.

## 기존 개발 인계

- `WI-ACTOR-03 지식 습득`: `Logic E4 / Presentation E3 / 통합 E3`
- 같은 WorldRevision의 지식 원장과 Preview를 `Known / Readable / Blocked` 처방 카드 상태로 결정적으로 투영하는 읽기 사본까지 구현했다.
- Save/Replay·실제 Unity Scene 배선·Recipe UI 조작·Play Mode·Game View·채집·달이기·섭취·약효는 아직 미구현이다.

## 다음 질문 후보

- 3×3 조합의 마우스·키보드·게임패드 입력
- 집중력 부족 시 Preview·Confirm 제한
- 공식 Recipe의 발견·분석·전수 UI

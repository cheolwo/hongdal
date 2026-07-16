const roles = [
  {
    key: "community",
    eyebrow: "COMMUNITY · ORDERER",
    title: "주문자·커뮤니티",
    description: "동네 글을 읽고 공동구매나 공동수입 제안을 구체적인 업무 흐름으로 연결합니다.",
    image: "/assets/images/community-orderer.png",
    imageAlt: "모바일 Hongdal 생활 게시판 화면",
    accent: "blue",
    startPath: "/community",
    screens: [
      { path: "/community", title: "생활 게시판", description: "동네 글과 댓글, 연결된 업무 원장을 함께 봅니다.", tag: "바로 보기" },
      { path: "/community/group-purchase", title: "공동구매 제안", description: "참여자와 수량, 가격, 공급 조건을 맞춥니다.", tag: "흐름 체험" },
      { path: "/community/group-import", title: "공동수입 검토", description: "해외 공급과 국내 수요를 수입 흐름으로 연결합니다.", tag: "흐름 체험" },
      { path: "/information/public-data", title: "농수산물 공개 정보", description: "공식 가격 자료와 출처를 비교해 봅니다.", tag: "바로 보기" }
    ]
  },
  {
    key: "global",
    eyebrow: "OVERSEAS SUPPLIER",
    title: "해외 공급자",
    description: "한국 시장에 상품을 소개하고 공급 조건과 수입 검토 가능성을 확인합니다.",
    image: "/assets/images/global-supplier.png",
    imageAlt: "Hongdal Global 상품 탐색 화면",
    accent: "teal",
    startPath: "/global",
    screens: [
      { path: "/global", title: "글로벌 상품", description: "한국 시장을 찾는 해외 상품과 공급 조건을 둘러봅니다.", tag: "바로 보기" },
      { path: "/global/suppliers/apply", title: "상품 제출", description: "회사, 상품, MOQ와 무역 조건 제출 흐름을 봅니다.", tag: "세션 체험" },
      { path: "/global/products/indonesian-rattan-storage-basket", title: "상품 상세", description: "샘플, HS 코드 제안과 수입 검토 정보를 확인합니다.", tag: "바로 보기" },
      { path: "/community/global-trade/101", title: "무역 대화", description: "공급자와 국내 참여자가 공개적으로 조건을 맞춥니다.", tag: "세션 체험" }
    ]
  },
  {
    key: "shipper",
    eyebrow: "SHIPPER · SELLER",
    title: "화주·판매자",
    description: "운송 의뢰부터 입고, 재고, 판매채널과 주문 출고까지 이어지는 업무를 봅니다.",
    image: "/assets/images/shipper-seller.png",
    imageAlt: "화주 역할 업무 선택 화면",
    accent: "green",
    startPath: "/shipper/request",
    screens: [
      { path: "/shipper/request", title: "운송 의뢰", description: "화물, 상하차지, 차량과 결제 조건을 입력합니다.", tag: "절차 체험" },
      { path: "/shipper/inbound/dashboard", title: "입고 대시보드", description: "입고 예정과 완료, 보관 재고를 한눈에 봅니다.", tag: "화면 체험" },
      { path: "/shipper/sales/channels", title: "판매채널", description: "판매채널 계정과 연결 상태를 관리합니다.", tag: "화면 체험" },
      { path: "/shipper/international/fcl-lcl", title: "통관·FCL/LCL", description: "수입량과 비용을 비교하고 운송 방식을 검토합니다.", tag: "화면 체험" }
    ]
  },
  {
    key: "driver",
    eyebrow: "CARGO DRIVER",
    title: "운송 기사",
    description: "추천 운송을 지도에서 확인하고 상차, 하차, 증빙과 정산을 처리합니다.",
    image: "/assets/images/driver.png",
    imageAlt: "운송 기사 추천 경로 지도 화면",
    accent: "cyan",
    startPath: "/driver/home",
    screens: [
      { path: "/driver/home", title: "기사 홈", description: "현재 위치와 진행 중 운송을 중심으로 봅니다.", tag: "화면 체험" },
      { path: "/driver/recommendations", title: "추천 운송", description: "거리, 예상 수익과 상하차 조건을 비교합니다.", tag: "절차 체험" },
      { path: "/driver/transports/current", title: "진행 중 운송", description: "수락한 운송의 현재 단계와 다음 행동을 확인합니다.", tag: "절차 체험" },
      { path: "/driver/transport/proof", title: "운송 증빙", description: "상차와 하차 사진, 예외 증빙 흐름을 확인합니다.", tag: "화면 체험" }
    ]
  },
  {
    key: "warehouse",
    eyebrow: "WAREHOUSE OPERATOR",
    title: "창고 관리자",
    description: "입고 확인, 검수, 적재, 피킹과 포장을 현장 순서대로 처리합니다.",
    image: "/assets/images/warehouse.png",
    imageAlt: "창고 관리자 역할 업무 선택 화면",
    accent: "olive",
    startPath: "/warehouse/work-board",
    screens: [
      { path: "/warehouse/work-board", title: "작업 보드", description: "입고와 출고 작업, 연결 대상을 한곳에서 봅니다.", tag: "화면 체험" },
      { path: "/warehouse/work/inbound/products", title: "입고 상품 확인", description: "상품 바코드로 입고 예정 항목을 찾습니다.", tag: "스캔 체험" },
      { path: "/warehouse/work/inbound/inspection", title: "입고 검수", description: "수량 차이, 파손과 보관 조건을 확인합니다.", tag: "절차 체험" },
      { path: "/warehouse/mart/picking", title: "마트 피킹·포장", description: "전달 주문을 피킹하고 포장 완료까지 처리합니다.", tag: "절차 체험" }
    ]
  }
];

const screenByPath = new Map();
for (const role of roles) {
  for (const screen of role.screens) {
    screenByPath.set(screen.path, { ...screen, role });
  }
}

const roleSamples = {
  community: [
    ["새 공동구매", "여름 제철 복숭아 공동구매", "참여 18명"],
    ["공동수입", "포르투갈 코르크 생활용품", "검토 중"],
    ["생활 질문", "동네 배송 묶음 제안", "댓글 12개"]
  ],
  global: [
    ["Portugal", "Modular Cork Desk Organizer", "MOQ 120"],
    ["Indonesia", "Handwoven Rattan Storage Basket", "MOQ 80"],
    ["Mexico", "Recycled Glass Table Vase", "MOQ 144"]
  ],
  shipper: [
    ["운송 준비", "서울 성동 → 부산 강서", "1.2톤"],
    ["입고 예정", "생활용품 24 SKU", "오늘 15:30"],
    ["판매채널", "주문 동기화", "정상"]
  ],
  driver: [
    ["추천 1", "4.8 km · 예상 42분", "예상 58,000원"],
    ["추천 2", "7.1 km · 예상 55분", "예상 72,000원"],
    ["진행 중", "상차지 도착 확인", "다음: 상차"]
  ],
  warehouse: [
    ["입고", "HD-IB-260717-14", "검수 대기"],
    ["피킹", "마트 주문 8건", "진행 중"],
    ["출고", "택배 인계 12박스", "16:00 마감"]
  ]
};

let selectedRoleKey = "community";
let toastTimer;

const app = document.querySelector("#app");
const toast = document.querySelector("#toast");
const dialog = document.querySelector("#demo-dialog");
const dialogCopy = document.querySelector("#demo-dialog-copy");

function roleLabel(role) {
  return `${role.title} 화면`;
}

function screenLink(screen, index) {
  const number = String(index + 1).padStart(2, "0");
  return `
    <a class="screen-link" href="${screen.path}" data-route>
      <span class="screen-link__number">${number}</span>
      <span class="screen-link__copy">
        <strong>${screen.title}</strong>
        <small>${screen.description}</small>
      </span>
      <span class="screen-link__tag">${screen.tag}</span>
      <span class="screen-link__arrow" aria-hidden="true">→</span>
    </a>`;
}

function renderHome() {
  const role = roles.find((item) => item.key === selectedRoleKey) || roles[0];
  document.title = "Hongdal 역할별 화면 체험";
  app.innerHTML = `
    <section class="experience experience--${role.accent}">
      <header class="experience__intro">
        <div>
          <span class="eyebrow">HONGDAL PRODUCT PREVIEW</span>
          <h1>역할별 화면 체험</h1>
          <p>현재 구현된 주요 흐름을 역할에 따라 열어보고, 실제 데이터 변경 없이 화면 동작을 살펴봅니다.</p>
        </div>
        <span class="read-only-badge">읽기 전용 · 5개 역할</span>
      </header>

      <nav class="role-selector" aria-label="체험할 역할 선택">
        ${roles.map((item) => `
          <button class="role-button ${item.key === role.key ? "is-selected" : ""}"
                  type="button"
                  data-role-key="${item.key}"
                  aria-pressed="${item.key === role.key}">
            <span class="role-button__index">${String(roles.indexOf(item) + 1).padStart(2, "0")}</span>
            ${item.title}
          </button>`).join("")}
      </nav>

      <div class="experience__workspace">
        <figure class="screen-preview">
          <img src="${role.image}" alt="${role.imageAlt}">
          <figcaption>
            <span>${role.eyebrow}</span>
            <strong>${role.title}</strong>
          </figcaption>
        </figure>

        <section class="role-detail" aria-label="${roleLabel(role)}">
          <header class="role-detail__header">
            <div>
              <span class="eyebrow">${role.eyebrow}</span>
              <h2>${role.title}</h2>
              <p>${role.description}</p>
            </div>
            <a class="primary-button" href="${role.startPath}" data-route>대표 화면 열기 <span aria-hidden="true">→</span></a>
          </header>
          <div class="screen-list">
            ${role.screens.map(screenLink).join("")}
          </div>
        </section>
      </div>

      <section class="preview-policy" aria-label="체험 범위">
        <div>
          <span class="eyebrow">SAFE PREVIEW</span>
          <h2>실제 업무를 바꾸지 않는 화면 체험</h2>
        </div>
        <p>로그인, 주문 확정, 배차 수락, 파일 업로드와 결제는 샘플 상태로만 반응합니다. 화면 이동과 버튼 동작은 공개 사이트 안에서 끝납니다.</p>
      </section>
    </section>`;

  bindHomeInteractions();
}

function renderScreen(entry) {
  const { role, title, description, tag, path } = entry;
  const samples = roleSamples[role.key];
  document.title = `${title} | Hongdal 체험`;
  app.innerHTML = `
    <section class="screen-page screen-page--${role.accent}">
      <nav class="breadcrumbs" aria-label="현재 위치">
        <a href="/" data-route>체험 홈</a>
        <span aria-hidden="true">/</span>
        <button type="button" data-select-role="${role.key}">${role.title}</button>
        <span aria-hidden="true">/</span>
        <strong>${title}</strong>
      </nav>

      <div class="screen-page__layout">
        <aside class="role-rail" aria-label="${role.title} 화면 목록">
          <span class="eyebrow">${role.eyebrow}</span>
          <h1>${role.title}</h1>
          <p>${role.description}</p>
          <nav>
            ${role.screens.map((screen) => `
              <a href="${screen.path}" data-route class="${screen.path === path ? "is-current" : ""}">
                <span>${screen.title}</span>
                <small>${screen.tag}</small>
              </a>`).join("")}
          </nav>
          <a class="secondary-button" href="/" data-route>다른 역할 보기</a>
        </aside>

        <article class="workbench">
          <header class="workbench__header">
            <div>
              <span class="eyebrow">${tag}</span>
              <h2>${title}</h2>
              <p>${description}</p>
            </div>
            <button class="primary-button" type="button" data-demo-action="${title}">체험 동작 실행</button>
          </header>

          <section class="workflow" aria-labelledby="workflow-title">
            <header>
              <div>
                <span class="eyebrow">WORKFLOW</span>
                <h3 id="workflow-title">업무 진행 단계</h3>
              </div>
              <strong id="step-summary">1단계 · 요청 확인</strong>
            </header>
            <div class="workflow__steps" role="group" aria-label="업무 단계 선택">
              ${["요청 확인", "조건 검토", "실행 준비", "완료 확인"].map((step, index) => `
                <button type="button" class="workflow-step ${index === 0 ? "is-active" : ""}" data-step="${index}" aria-pressed="${index === 0}">
                  <span>${String(index + 1).padStart(2, "0")}</span>
                  ${step}
                </button>`).join("")}
            </div>
          </section>

          <section class="sample-section" aria-labelledby="sample-title">
            <header>
              <div>
                <span class="eyebrow">SAMPLE DATA</span>
                <h3 id="sample-title">대표 항목</h3>
              </div>
              <button class="text-button" type="button" data-refresh-sample>샘플 새로고침</button>
            </header>
            <div class="sample-list">
              ${samples.map((row, index) => `
                <div class="sample-row">
                  <span class="sample-row__index">${String(index + 1).padStart(2, "0")}</span>
                  <div>
                    <small>${row[0]}</small>
                    <strong>${row[1]}</strong>
                  </div>
                  <span class="sample-row__status">${row[2]}</span>
                </div>`).join("")}
            </div>
          </section>

          <section class="screen-note">
            <div>
              <span class="eyebrow">PREVIEW MODE</span>
              <h3>안전한 공개 체험</h3>
            </div>
            <p>이 페이지의 동작은 현재 브라우저에서만 보이며 서버 데이터, 주문, 운송, 재고와 결제를 변경하지 않습니다.</p>
          </section>
        </article>
      </div>
    </section>`;

  bindScreenInteractions(role);
}

function renderUnknown(path) {
  document.title = "준비 중인 화면 | Hongdal 체험";
  app.innerHTML = `
    <section class="unknown-page">
      <span class="eyebrow">PREVIEW ROUTE</span>
      <h1>이 경로는 아직 체험 목록에 없습니다.</h1>
      <p><code>${path}</code> 대신 역할별 대표 화면에서 현재 공개된 흐름을 확인할 수 있습니다.</p>
      <div class="unknown-page__actions">
        <a class="primary-button" href="/" data-route>체험 홈으로</a>
        <a class="secondary-button" href="https://github.com/cheolwo/hongdal" target="_blank" rel="noreferrer">GitHub 보기</a>
      </div>
    </section>`;
}

function bindHomeInteractions() {
  document.querySelectorAll("[data-role-key]").forEach((button) => {
    button.addEventListener("click", () => {
      selectedRoleKey = button.dataset.roleKey;
      renderHome();
      document.querySelector(`[data-role-key="${selectedRoleKey}"]`)?.focus();
    });
  });
}

function bindScreenInteractions(role) {
  document.querySelector("[data-select-role]")?.addEventListener("click", () => {
    selectedRoleKey = role.key;
    navigate("/");
  });

  document.querySelectorAll("[data-step]").forEach((button) => {
    button.addEventListener("click", () => {
      document.querySelectorAll("[data-step]").forEach((item) => {
        const isActive = item === button;
        item.classList.toggle("is-active", isActive);
        item.setAttribute("aria-pressed", String(isActive));
      });
      const stepNames = ["요청 확인", "조건 검토", "실행 준비", "완료 확인"];
      const index = Number(button.dataset.step);
      document.querySelector("#step-summary").textContent = `${index + 1}단계 · ${stepNames[index]}`;
      showToast(`${stepNames[index]} 단계를 미리 보고 있습니다.`);
    });
  });

  document.querySelector("[data-demo-action]")?.addEventListener("click", (event) => {
    dialogCopy.textContent = `${event.currentTarget.dataset.demoAction} 동작을 샘플 상태로 확인했습니다. 실제 데이터는 변경되지 않습니다.`;
    if (typeof dialog.showModal === "function") dialog.showModal();
    else dialog.setAttribute("open", "");
  });

  document.querySelector("[data-refresh-sample]")?.addEventListener("click", (event) => {
    event.currentTarget.textContent = "샘플 확인 완료";
    showToast("최신 샘플 상태를 확인했습니다.");
  });
}

function render() {
  const path = normalizePath(window.location.pathname);
  if (path === "/") renderHome();
  else if (screenByPath.has(path)) renderScreen(screenByPath.get(path));
  else renderUnknown(path);
  bindRouteLinks();
}

function normalizePath(path) {
  if (!path || path === "/") return "/";
  return path.replace(/\/+$/, "") || "/";
}

function navigate(path) {
  const target = normalizePath(path);
  if (normalizePath(window.location.pathname) !== target) {
    window.history.pushState({}, "", target);
  }
  render();
  window.scrollTo({ top: 0, behavior: "instant" });
  app.focus({ preventScroll: true });
}

function bindRouteLinks() {
  document.querySelectorAll("a[data-route]").forEach((link) => {
    link.addEventListener("click", (event) => {
      if (event.defaultPrevented || event.button !== 0 || event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) return;
      event.preventDefault();
      navigate(new URL(link.href, window.location.origin).pathname);
    });
  });
}

function showToast(message) {
  window.clearTimeout(toastTimer);
  toast.textContent = message;
  toast.classList.add("is-visible");
  toastTimer = window.setTimeout(() => toast.classList.remove("is-visible"), 2600);
}

document.querySelector("[data-close-dialog]")?.addEventListener("click", () => dialog.close());
window.addEventListener("popstate", render);
render();


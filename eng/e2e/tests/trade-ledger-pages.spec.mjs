import { expect, test } from "@playwright/test";

const screens = [
  {
    path: "/orderer/ledgers/individual-import",
    heading: "개별수입 원장",
    field: "원천 개별주문 원장 ID",
    boundaryText: "실행하지 않습니다"
  },
  {
    path: "/orderer/ledgers/individual-export",
    heading: "개별수출 원장",
    field: "원천 개별주문 원장 ID",
    boundaryText: "별도 승인 전까지 차단됩니다"
  },
  {
    path: "/orderer/ledgers/group-export",
    heading: "공동수출 원장",
    field: "개별수출 원장 ID",
    boundaryText: "개별수출 원장에 그대로 보존됩니다"
  }
];

for (const screen of screens) {
  test(`${screen.heading} 화면이 실행 경계를 포함해 렌더링된다`, async ({ page }) => {
    const fatalConsoleErrors = [];
    page.on("console", message => {
      if (message.type() === "error" && !message.text().includes("Failed to load resource")) {
        fatalConsoleErrors.push(message.text());
      }
    });

    await page.addInitScript(snapshot => {
      localStorage.setItem("ssalddel.web.auth.v1", JSON.stringify(snapshot));
    }, {
      accessToken: "browser-e2e-simulation-token",
      accessTokenExpiresAtUtc: "2099-01-01T00:00:00Z",
      refreshToken: "browser-e2e-refresh-token",
      refreshTokenExpiresAtUtc: "2099-01-02T00:00:00Z",
      userId: "browser-e2e-admin",
      userName: "browser-e2e-admin",
      roles: ["서버관리자"],
      preferredLanguageCode: "ko"
    });

    await page.goto(screen.path);
    await expect(page.getByRole("heading", { name: screen.heading })).toBeVisible();
    await expect(page.getByText(screen.boundaryText, { exact: false })).toBeVisible();
    await expect(page.getByLabel(screen.field, { exact: false })).toBeVisible();
    await expect(page.locator("#blazor-error-ui")).not.toBeVisible();
    expect(fatalConsoleErrors).toEqual([]);
  });
}

test("로그인하지 않은 사용자는 원장 입력 대신 로그인 안내를 본다", async ({ page }) => {
  await page.goto("/orderer/ledgers/individual-import");
  await expect(page.getByText("원장을 만들려면 로그인이 필요합니다.")).toBeVisible();
  await expect(page.getByRole("link", { name: "로그인하고 계속하기" })).toHaveAttribute(
    "href",
    /returnUrl=%2Forderer%2Fledgers%2Findividual-import/);
  await expect(page.getByLabel("원천 개별주문 원장 ID", { exact: false })).toHaveCount(0);
});

test("주문자 1.5 홈이 핵심 점검 경로를 제공한다", async ({ page }) => {
  await page.goto("/orderer");
  await expect(page.getByRole("heading", { name: "주문자 1.5 점검 작업공간" })).toBeVisible();
  await expect(page.getByText("개별수입 원장", { exact: true })).toBeVisible();
  await expect(page.getByText("개별수출 원장", { exact: true })).toBeVisible();
  await expect(page.getByText("공동수출 원장", { exact: true })).toBeVisible();
  await expect(page.getByText("계약·결제·신고 제출", { exact: false })).toBeVisible();
});

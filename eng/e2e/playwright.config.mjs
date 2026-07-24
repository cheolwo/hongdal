import { defineConfig, devices } from "@playwright/test";

const baseURL = process.env.SSALDDEL_E2E_BASE_URL ?? "http://127.0.0.1:4173";

export default defineConfig({
  testDir: "./tests",
  outputDir: "../../artifacts/browser-e2e/results",
  reporter: [
    ["line"],
    ["html", { outputFolder: "../../artifacts/browser-e2e/report", open: "never" }]
  ],
  use: {
    baseURL,
    screenshot: "only-on-failure",
    trace: "retain-on-failure"
  },
  projects: [
    { name: "desktop-chromium", use: { ...devices["Desktop Chrome"] } },
    { name: "mobile-chromium", use: { ...devices["Pixel 7"] } }
  ],
  webServer: process.env.SSALDDEL_E2E_BASE_URL
    ? undefined
    : {
        command: "npx serve -s ../../Ssalddel.WebApp/bin/Release/net10.0/wwwroot -l 4173",
        url: baseURL,
        reuseExistingServer: true,
        timeout: 120_000
      }
});

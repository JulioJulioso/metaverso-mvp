import { defineConfig } from "@playwright/test";

const target = process.env.METAVERSO_URL || "http://127.0.0.1:4173";

export default defineConfig({
  testDir: ".",
  testMatch: "smoke.spec.js",
  timeout: 30000,
  use: { baseURL: target },
  webServer: process.env.METAVERSO_URL
    ? undefined
    : {
        command: "node serve-docs.mjs",
        url: "http://127.0.0.1:4173/index.html",
        reuseExistingServer: true,
      },
});

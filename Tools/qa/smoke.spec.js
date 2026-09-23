import { test, expect } from "@playwright/test";

test("el shell avisa que no es una aplicacion y elige Quest o PC", async ({ request }) => {
  const response = await request.get("/index.html");
  const html = await response.text();
  expect(html).toContain("No se instala una aplicacion");
  expect(html).toContain("OculusBrowser");
  expect(html).toContain("quest/");
  expect(html).toContain("desktop/");
});

test("un build de Unity expone el canvas", async ({ page }) => {
  test.skip(!process.env.METAVERSO_URL, "Define METAVERSO_URL cuando exista el build de Unity.");
  await page.goto("/");
  await expect(page.locator("canvas")).toBeVisible({ timeout: 20000 });
});

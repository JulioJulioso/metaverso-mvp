import { readFileSync, existsSync, readdirSync, statSync } from "node:fs";
import { basename, dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const root = join(dirname(fileURLToPath(import.meta.url)), "..", "..");
const errors = [];

function mustInclude(file, needles) {
  const absolute = join(root, file);
  if (!existsSync(absolute)) {
    errors.push("falta " + file);
    return;
  }
  const text = readFileSync(absolute, "utf8");
  for (const needle of needles) {
    if (!text.includes(needle))
      errors.push(file + " no contiene: " + needle);
  }
}

function mustNotExist(file) {
  if (existsSync(join(root, file)))
    errors.push("deberia no existir: " + file);
}

mustInclude("metaverso-web/docs/index.html", [
  "No se instala una aplicacion",
  "OculusBrowser",
  "quest/",
  "desktop/",
]);
mustInclude("metaverso-web/docs/version.json", ["version", "label"]);
mustInclude("metaverso-web/embed/iframe.html", ["xr-spatial-tracking", "allowfullscreen"]);

mustInclude("README.md", ["6000.3.9f1", "Docs/ARQUITECTURA.md", "Git LFS"]);
mustInclude("ROADMAP.md", ["Photon", "Quest", "Decisiones"]);
for (const doc of ["ARQUITECTURA", "PIPELINE_MODELOS", "DEPLOY", "MULTIPLAYER", "QA", "ESTADO_DEL_ARTE", "EVALUACION_COMERCIAL"])
  mustInclude(`Docs/${doc}.md`, ["#"]);

mustInclude("Packages/manifest.json", ["com.de-panther.webxr", "com.unity.xr.interaction.toolkit"]);
mustInclude("ProjectSettings/EditorBuildSettings.asset", ["Assets/_Metaverso/Scenes/Circuito.unity"]);
mustInclude("Assets/_Metaverso/Scripts/XR/PlatformModeController.cs", ["ClientMode.Vr"]);
mustInclude("Assets/_Metaverso/Scripts/Editor/WebBuildPipeline.cs", ["ASTC", "DXTC"]);

mustNotExist("Assets/TutorialInfo");
mustNotExist("Assets/Readme.asset");
mustNotExist("Assets/Scenes/SampleScene.unity");

function walk(dir, out) {
  for (const name of readdirSync(dir)) {
    const full = join(dir, name);
    if (statSync(full).isDirectory()) walk(full, out);
    else if (name.endsWith(".cs")) out.push(full);
  }
  return out;
}

const componentPattern = /class\s+(\w+)\s*:\s*(MonoBehaviour|NetworkBehaviour|ScriptableObject)\b/g;
for (const file of walk(join(root, "Assets/_Metaverso/Scripts"), [])) {
  const expected = basename(file, ".cs");
  for (const match of readFileSync(file, "utf8").matchAll(componentPattern)) {
    if (match[1] !== expected)
      errors.push(`${expected}.cs declara ${match[2]} ${match[1]}: Unity no lo puede guardar en escenas`);
  }
}

const version = JSON.parse(readFileSync(join(root, "metaverso-web/docs/version.json"), "utf8"));
if (!version.version || !version.label)
  errors.push("version.json incompleto");

if (errors.length) {
  console.error(errors.join("\n"));
  process.exit(1);
}

console.log("check-shell ok:", version.label);

import { createServer } from "node:http";
import { readFile } from "node:fs/promises";
import { extname, join, normalize } from "node:path";
import { fileURLToPath } from "node:url";

const root = join(fileURLToPath(new URL(".", import.meta.url)), "..", "..", "metaverso-web", "docs");
const types = { ".html": "text/html", ".json": "application/json", ".js": "text/javascript" };

createServer(async (req, res) => {
  const url = new URL(req.url, "http://127.0.0.1");
  const path = normalize(join(root, decodeURIComponent(url.pathname)));
  if (!path.startsWith(root)) {
    res.writeHead(403);
    res.end();
    return;
  }
  try {
    const body = await readFile(path.endsWith("\\") || path.endsWith("/") ? join(path, "index.html") : path);
    res.writeHead(200, { "content-type": types[extname(path)] || "text/html" });
    res.end(body);
  } catch {
    res.writeHead(404);
    res.end("not found");
  }
}).listen(4173, "127.0.0.1");

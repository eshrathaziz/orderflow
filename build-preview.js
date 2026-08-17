import { cp, mkdir, rm, writeFile } from "node:fs/promises";
import { fileURLToPath } from "node:url";

const source = fileURLToPath(new URL("./preview/", import.meta.url));
const destination = fileURLToPath(new URL("./dist/public/", import.meta.url));

await rm(destination, { recursive: true, force: true });
await mkdir(destination, { recursive: true });
await cp(source, destination, { recursive: true });
await writeFile(fileURLToPath(new URL("./dist/index.js", import.meta.url)), `import { createReadStream, existsSync, statSync } from "node:fs";
import { createServer } from "node:http";
import { extname, join } from "node:path";
import { fileURLToPath } from "node:url";

const root = fileURLToPath(new URL("./public/", import.meta.url));
const port = Number(process.env.PORT || 3000);
const mimeTypes = { ".html": "text/html; charset=utf-8", ".css": "text/css; charset=utf-8", ".js": "text/javascript; charset=utf-8", ".json": "application/json; charset=utf-8" };

createServer((request, response) => {
  const pathname = request.url?.split("?")[0] || "/";
  const requested = pathname === "/" ? "index.html" : pathname.replace(/^\\/+/, "");
  const filePath = join(root, requested);
  if (!filePath.startsWith(root) || !existsSync(filePath) || !statSync(filePath).isFile()) {
    response.writeHead(404, { "Content-Type": "text/plain; charset=utf-8" });
    response.end("Not found");
    return;
  }
  response.writeHead(200, { "Content-Type": mimeTypes[extname(filePath)] || "application/octet-stream", "Cache-Control": "no-store" });
  const stream = createReadStream(filePath);
  stream.on("error", () => { if (!response.headersSent) response.writeHead(500, { "Content-Type": "text/plain; charset=utf-8" }); response.end("Unable to read preview file"); });
  stream.pipe(response);
}).listen(port, "0.0.0.0", () => console.log("OrderFlow production preview listening on " + port));
`);
console.log("OrderFlow development preview written to dist/public with a production startup entry point.");

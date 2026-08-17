import { cp, mkdir, rm } from "node:fs/promises";
import { fileURLToPath } from "node:url";

const source = fileURLToPath(new URL("./preview/", import.meta.url));
const destination = fileURLToPath(new URL("./dist/public/", import.meta.url));

await rm(destination, { recursive: true, force: true });
await mkdir(destination, { recursive: true });
await cp(source, destination, { recursive: true });
console.log("OrderFlow development preview written to dist/public.");

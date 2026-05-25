// vite.config.ts
import { defineConfig } from "file:///D:/%E5%B7%A5%E4%BD%9C%E5%8F%B0/C%23%E9%AB%98%E7%BA%A7%E7%BC%96%E7%A8%8B%E5%AD%A6%E4%B9%A0/NewLearnEnglish/LearnEnglish/ClientApp/node_modules/vite/dist/node/index.js";
import react from "file:///D:/%E5%B7%A5%E4%BD%9C%E5%8F%B0/C%23%E9%AB%98%E7%BA%A7%E7%BC%96%E7%A8%8B%E5%AD%A6%E4%B9%A0/NewLearnEnglish/LearnEnglish/ClientApp/node_modules/@vitejs/plugin-react/dist/index.js";
import path from "path";
import fs from "fs";
var __vite_injected_original_dirname = "L:\\";
var srcDir = path.resolve(__vite_injected_original_dirname, "./src");
function hashPathFixPlugin() {
  const extensions = [".ts", ".tsx", ".js", ".jsx", ".json", ".css"];
  const indexFiles = extensions.map((ext) => "index" + ext);
  function tryResolve(base) {
    if (fs.existsSync(base)) {
      const stat = fs.statSync(base);
      if (stat.isFile()) return base;
      for (const idx of indexFiles) {
        const p = path.join(base, idx);
        if (fs.existsSync(p)) return p;
      }
      return null;
    }
    for (const ext of extensions) {
      const p = base + ext;
      if (fs.existsSync(p)) return p;
    }
    return null;
  }
  const emptyStub = path.join(srcDir, "stubs/empty.ts");
  const nodeStubs = {
    stream: path.join(srcDir, "stubs/stream.ts"),
    util: path.join(srcDir, "stubs/util.ts"),
    events: path.join(srcDir, "stubs/events.ts"),
    http: emptyStub,
    https: emptyStub,
    zlib: emptyStub,
    url: emptyStub,
    assert: emptyStub,
    tty: emptyStub,
    os: emptyStub,
    net: emptyStub,
    fs: emptyStub,
    path: emptyStub,
    crypto: emptyStub,
    dns: emptyStub,
    child_process: emptyStub,
    "follow-redirects": emptyStub
  };
  return {
    name: "hash-path-fix",
    enforce: "pre",
    // 仅在 build 模式下启用（dev 模式用 resolve.alias，不受 # 路径影响）
    apply: "build",
    resolveId(source, importer) {
      if (nodeStubs[source]) return nodeStubs[source];
      if (source.startsWith("@/")) {
        const rest = source.slice(2);
        const resolved = tryResolve(path.join(srcDir, rest));
        return resolved;
      }
      if (importer && source.startsWith(".")) {
        const importerDir = path.dirname(importer);
        const resolved = tryResolve(path.resolve(importerDir, source));
        return resolved;
      }
      return null;
    }
  };
}
var vite_config_default = defineConfig({
  plugins: [hashPathFixPlugin(), react()],
  resolve: {
    alias: {
      "@": srcDir
    }
  },
  server: {
    port: 5173,
    proxy: {
      "/api": { target: "https://localhost:7051", secure: false, changeOrigin: true },
      "/course": { target: "https://localhost:7051", secure: false, changeOrigin: true },
      "/word": { target: "https://localhost:7051", secure: false, changeOrigin: true },
      "/exam": { target: "https://localhost:7051", secure: false, changeOrigin: true },
      "/examv2": { target: "https://localhost:7051", secure: false, changeOrigin: true },
      "/hearing": { target: "https://localhost:7051", secure: false, changeOrigin: true },
      "/whisper": { target: "https://localhost:7051", secure: false, changeOrigin: true },
      "/login": { target: "https://localhost:7051", secure: false, changeOrigin: true },
      "/home": { target: "https://localhost:7051", secure: false, changeOrigin: true },
      "/statistics": { target: "https://localhost:7051", secure: false, changeOrigin: true },
      "/translate": { target: "https://localhost:7051", secure: false, changeOrigin: true },
      "/userinfo": { target: "https://localhost:7051", secure: false, changeOrigin: true },
      "/import": { target: "https://localhost:7051", secure: false, changeOrigin: true }
    }
  },
  build: {
    outDir: "../wwwroot/spa",
    emptyOutDir: true,
    rollupOptions: {
      onwarn(warning, warn) {
        if (warning.code === "MODULE_LEVEL_DIRECTIVE") return;
        warn(warning);
      }
    }
  }
});
export {
  vite_config_default as default
};
//# sourceMappingURL=data:application/json;base64,ewogICJ2ZXJzaW9uIjogMywKICAic291cmNlcyI6IFsidml0ZS5jb25maWcudHMiXSwKICAic291cmNlc0NvbnRlbnQiOiBbImNvbnN0IF9fdml0ZV9pbmplY3RlZF9vcmlnaW5hbF9kaXJuYW1lID0gXCJMOlxcXFxcIjtjb25zdCBfX3ZpdGVfaW5qZWN0ZWRfb3JpZ2luYWxfZmlsZW5hbWUgPSBcIkw6XFxcXHZpdGUuY29uZmlnLnRzXCI7Y29uc3QgX192aXRlX2luamVjdGVkX29yaWdpbmFsX2ltcG9ydF9tZXRhX3VybCA9IFwiZmlsZTovLy9MOi92aXRlLmNvbmZpZy50c1wiO2ltcG9ydCB7IGRlZmluZUNvbmZpZyB9IGZyb20gJ3ZpdGUnXG5pbXBvcnQgcmVhY3QgZnJvbSAnQHZpdGVqcy9wbHVnaW4tcmVhY3QnXG5pbXBvcnQgcGF0aCBmcm9tICdwYXRoJ1xuaW1wb3J0IGZzIGZyb20gJ2ZzJ1xuXG5jb25zdCBzcmNEaXIgPSBwYXRoLnJlc29sdmUoX19kaXJuYW1lLCAnLi9zcmMnKVxuXG4vLyBcdTg5RTNcdTUxQjNcdThERUZcdTVGODRcdTRFMkRcdTU0MkIgIyBcdTVCNTdcdTdCMjZcdTVCRkNcdTgxRjQgUm9sbHVwL1ZpdGUgXHU2MjY5XHU1QzU1XHU1NDBEXHU4OUUzXHU2NzkwXHU1OTMxXHU4RDI1XHU3Njg0XHU5NUVFXHU5ODk4XG5mdW5jdGlvbiBoYXNoUGF0aEZpeFBsdWdpbigpIHtcbiAgY29uc3QgZXh0ZW5zaW9ucyA9IFsnLnRzJywgJy50c3gnLCAnLmpzJywgJy5qc3gnLCAnLmpzb24nLCAnLmNzcyddXG4gIGNvbnN0IGluZGV4RmlsZXMgPSBleHRlbnNpb25zLm1hcChleHQgPT4gJ2luZGV4JyArIGV4dClcblxuICBmdW5jdGlvbiB0cnlSZXNvbHZlKGJhc2U6IHN0cmluZyk6IHN0cmluZyB8IG51bGwge1xuICAgIC8vIFx1NzZGNFx1NjNBNVx1NTMzOVx1OTE0RFx1RkYwOFx1NUUyNlx1NjI2OVx1NUM1NVx1NTQwRFx1RkYwOVxuICAgIGlmIChmcy5leGlzdHNTeW5jKGJhc2UpKSB7XG4gICAgICBjb25zdCBzdGF0ID0gZnMuc3RhdFN5bmMoYmFzZSlcbiAgICAgIGlmIChzdGF0LmlzRmlsZSgpKSByZXR1cm4gYmFzZVxuICAgICAgLy8gXHU2NTg3XHU0RUY2XHU1OTM5XHU1MjE5XHU2N0U1IGluZGV4XG4gICAgICBmb3IgKGNvbnN0IGlkeCBvZiBpbmRleEZpbGVzKSB7XG4gICAgICAgIGNvbnN0IHAgPSBwYXRoLmpvaW4oYmFzZSwgaWR4KVxuICAgICAgICBpZiAoZnMuZXhpc3RzU3luYyhwKSkgcmV0dXJuIHBcbiAgICAgIH1cbiAgICAgIHJldHVybiBudWxsXG4gICAgfVxuICAgIC8vIFx1NUMxRFx1OEJENVx1NTJBMFx1NjI2OVx1NUM1NVx1NTQwRFxuICAgIGZvciAoY29uc3QgZXh0IG9mIGV4dGVuc2lvbnMpIHtcbiAgICAgIGNvbnN0IHAgPSBiYXNlICsgZXh0XG4gICAgICBpZiAoZnMuZXhpc3RzU3luYyhwKSkgcmV0dXJuIHBcbiAgICB9XG4gICAgcmV0dXJuIG51bGxcbiAgfVxuXG4gIC8vIE5vZGUuanMgYnVpbHQtaW4gXHU2ODY5XHU2NjIwXHU1QzA0XHVGRjA4XHU4OUUzXHU1MUIzIEF4aW9zIFx1N0I0OVx1NUU5M1x1NTcyOFx1NkQ0Rlx1ODlDOFx1NTY2OFx1Njc4NFx1NUVGQVx1NEUyRFx1NzY4NFx1NTE3Q1x1NUJCOVx1OTVFRVx1OTg5OFx1RkYwOVxuICBjb25zdCBlbXB0eVN0dWIgPSBwYXRoLmpvaW4oc3JjRGlyLCAnc3R1YnMvZW1wdHkudHMnKVxuICBjb25zdCBub2RlU3R1YnM6IFJlY29yZDxzdHJpbmcsIHN0cmluZz4gPSB7XG4gICAgc3RyZWFtOiBwYXRoLmpvaW4oc3JjRGlyLCAnc3R1YnMvc3RyZWFtLnRzJyksXG4gICAgdXRpbDogcGF0aC5qb2luKHNyY0RpciwgJ3N0dWJzL3V0aWwudHMnKSxcbiAgICBldmVudHM6IHBhdGguam9pbihzcmNEaXIsICdzdHVicy9ldmVudHMudHMnKSxcbiAgICBodHRwOiBlbXB0eVN0dWIsXG4gICAgaHR0cHM6IGVtcHR5U3R1YixcbiAgICB6bGliOiBlbXB0eVN0dWIsXG4gICAgdXJsOiBlbXB0eVN0dWIsXG4gICAgYXNzZXJ0OiBlbXB0eVN0dWIsXG4gICAgdHR5OiBlbXB0eVN0dWIsXG4gICAgb3M6IGVtcHR5U3R1YixcbiAgICBuZXQ6IGVtcHR5U3R1YixcbiAgICBmczogZW1wdHlTdHViLFxuICAgIHBhdGg6IGVtcHR5U3R1YixcbiAgICBjcnlwdG86IGVtcHR5U3R1YixcbiAgICBkbnM6IGVtcHR5U3R1YixcbiAgICBjaGlsZF9wcm9jZXNzOiBlbXB0eVN0dWIsXG4gICAgJ2ZvbGxvdy1yZWRpcmVjdHMnOiBlbXB0eVN0dWIsXG4gIH1cblxuICByZXR1cm4ge1xuICAgIG5hbWU6ICdoYXNoLXBhdGgtZml4JyxcbiAgICBlbmZvcmNlOiAncHJlJyBhcyBjb25zdCxcbiAgICAvLyBcdTRFQzVcdTU3MjggYnVpbGQgXHU2QTIxXHU1RjBGXHU0RTBCXHU1NDJGXHU3NTI4XHVGRjA4ZGV2IFx1NkEyMVx1NUYwRlx1NzUyOCByZXNvbHZlLmFsaWFzXHVGRjBDXHU0RTBEXHU1M0Q3ICMgXHU4REVGXHU1Rjg0XHU1RjcxXHU1NENEXHVGRjA5XG4gICAgYXBwbHk6ICdidWlsZCcgYXMgY29uc3QsXG4gICAgcmVzb2x2ZUlkKHNvdXJjZTogc3RyaW5nLCBpbXBvcnRlcjogc3RyaW5nIHwgdW5kZWZpbmVkKSB7XG4gICAgICAvLyBOb2RlIGJ1aWx0LWluIFx1Njg2OVxuICAgICAgaWYgKG5vZGVTdHVic1tzb3VyY2VdKSByZXR1cm4gbm9kZVN0dWJzW3NvdXJjZV1cblxuICAgICAgLy8gXHU1OTA0XHU3NDA2IEAgXHU1MjJCXHU1NDBEXG4gICAgICBpZiAoc291cmNlLnN0YXJ0c1dpdGgoJ0AvJykpIHtcbiAgICAgICAgY29uc3QgcmVzdCA9IHNvdXJjZS5zbGljZSgyKVxuICAgICAgICBjb25zdCByZXNvbHZlZCA9IHRyeVJlc29sdmUocGF0aC5qb2luKHNyY0RpciwgcmVzdCkpXG4gICAgICAgIHJldHVybiByZXNvbHZlZFxuICAgICAgfVxuICAgICAgLy8gXHU1OTA0XHU3NDA2XHU3NkY4XHU1QkY5XHU1QkZDXHU1MTY1XG4gICAgICBpZiAoaW1wb3J0ZXIgJiYgc291cmNlLnN0YXJ0c1dpdGgoJy4nKSkge1xuICAgICAgICBjb25zdCBpbXBvcnRlckRpciA9IHBhdGguZGlybmFtZShpbXBvcnRlcilcbiAgICAgICAgY29uc3QgcmVzb2x2ZWQgPSB0cnlSZXNvbHZlKHBhdGgucmVzb2x2ZShpbXBvcnRlckRpciwgc291cmNlKSlcbiAgICAgICAgcmV0dXJuIHJlc29sdmVkXG4gICAgICB9XG4gICAgICByZXR1cm4gbnVsbFxuICAgIH0sXG4gIH1cbn1cblxuZXhwb3J0IGRlZmF1bHQgZGVmaW5lQ29uZmlnKHtcbiAgcGx1Z2luczogW2hhc2hQYXRoRml4UGx1Z2luKCksIHJlYWN0KCldLFxuICByZXNvbHZlOiB7XG4gICAgYWxpYXM6IHtcbiAgICAgICdAJzogc3JjRGlyLFxuICAgIH0sXG4gIH0sXG4gIHNlcnZlcjoge1xuICAgIHBvcnQ6IDUxNzMsXG4gICAgcHJveHk6IHtcbiAgICAgICcvYXBpJzogeyB0YXJnZXQ6ICdodHRwczovL2xvY2FsaG9zdDo3MDUxJywgc2VjdXJlOiBmYWxzZSwgY2hhbmdlT3JpZ2luOiB0cnVlIH0sXG4gICAgICAnL2NvdXJzZSc6IHsgdGFyZ2V0OiAnaHR0cHM6Ly9sb2NhbGhvc3Q6NzA1MScsIHNlY3VyZTogZmFsc2UsIGNoYW5nZU9yaWdpbjogdHJ1ZSB9LFxuICAgICAgJy93b3JkJzogeyB0YXJnZXQ6ICdodHRwczovL2xvY2FsaG9zdDo3MDUxJywgc2VjdXJlOiBmYWxzZSwgY2hhbmdlT3JpZ2luOiB0cnVlIH0sXG4gICAgICAnL2V4YW0nOiB7IHRhcmdldDogJ2h0dHBzOi8vbG9jYWxob3N0OjcwNTEnLCBzZWN1cmU6IGZhbHNlLCBjaGFuZ2VPcmlnaW46IHRydWUgfSxcbiAgICAgICcvZXhhbXYyJzogeyB0YXJnZXQ6ICdodHRwczovL2xvY2FsaG9zdDo3MDUxJywgc2VjdXJlOiBmYWxzZSwgY2hhbmdlT3JpZ2luOiB0cnVlIH0sXG4gICAgICAnL2hlYXJpbmcnOiB7IHRhcmdldDogJ2h0dHBzOi8vbG9jYWxob3N0OjcwNTEnLCBzZWN1cmU6IGZhbHNlLCBjaGFuZ2VPcmlnaW46IHRydWUgfSxcbiAgICAgICcvd2hpc3Blcic6IHsgdGFyZ2V0OiAnaHR0cHM6Ly9sb2NhbGhvc3Q6NzA1MScsIHNlY3VyZTogZmFsc2UsIGNoYW5nZU9yaWdpbjogdHJ1ZSB9LFxuICAgICAgJy9sb2dpbic6IHsgdGFyZ2V0OiAnaHR0cHM6Ly9sb2NhbGhvc3Q6NzA1MScsIHNlY3VyZTogZmFsc2UsIGNoYW5nZU9yaWdpbjogdHJ1ZSB9LFxuICAgICAgJy9ob21lJzogeyB0YXJnZXQ6ICdodHRwczovL2xvY2FsaG9zdDo3MDUxJywgc2VjdXJlOiBmYWxzZSwgY2hhbmdlT3JpZ2luOiB0cnVlIH0sXG4gICAgICAnL3N0YXRpc3RpY3MnOiB7IHRhcmdldDogJ2h0dHBzOi8vbG9jYWxob3N0OjcwNTEnLCBzZWN1cmU6IGZhbHNlLCBjaGFuZ2VPcmlnaW46IHRydWUgfSxcbiAgICAgICcvdHJhbnNsYXRlJzogeyB0YXJnZXQ6ICdodHRwczovL2xvY2FsaG9zdDo3MDUxJywgc2VjdXJlOiBmYWxzZSwgY2hhbmdlT3JpZ2luOiB0cnVlIH0sXG4gICAgICAnL3VzZXJpbmZvJzogeyB0YXJnZXQ6ICdodHRwczovL2xvY2FsaG9zdDo3MDUxJywgc2VjdXJlOiBmYWxzZSwgY2hhbmdlT3JpZ2luOiB0cnVlIH0sXG4gICAgICAnL2ltcG9ydCc6IHsgdGFyZ2V0OiAnaHR0cHM6Ly9sb2NhbGhvc3Q6NzA1MScsIHNlY3VyZTogZmFsc2UsIGNoYW5nZU9yaWdpbjogdHJ1ZSB9LFxuICAgIH0sXG4gIH0sXG4gIGJ1aWxkOiB7XG4gICAgb3V0RGlyOiAnLi4vd3d3cm9vdC9zcGEnLFxuICAgIGVtcHR5T3V0RGlyOiB0cnVlLFxuICAgIHJvbGx1cE9wdGlvbnM6IHtcbiAgICAgIG9ud2Fybih3YXJuaW5nLCB3YXJuKSB7XG4gICAgICAgIGlmICh3YXJuaW5nLmNvZGUgPT09ICdNT0RVTEVfTEVWRUxfRElSRUNUSVZFJykgcmV0dXJuO1xuICAgICAgICB3YXJuKHdhcm5pbmcpO1xuICAgICAgfSxcbiAgICB9LFxuICB9LFxufSlcbiJdLAogICJtYXBwaW5ncyI6ICI7QUFBNEwsU0FBUyxvQkFBb0I7QUFDek4sT0FBTyxXQUFXO0FBQ2xCLE9BQU8sVUFBVTtBQUNqQixPQUFPLFFBQVE7QUFIZixJQUFNLG1DQUFtQztBQUt6QyxJQUFNLFNBQVMsS0FBSyxRQUFRLGtDQUFXLE9BQU87QUFHOUMsU0FBUyxvQkFBb0I7QUFDM0IsUUFBTSxhQUFhLENBQUMsT0FBTyxRQUFRLE9BQU8sUUFBUSxTQUFTLE1BQU07QUFDakUsUUFBTSxhQUFhLFdBQVcsSUFBSSxTQUFPLFVBQVUsR0FBRztBQUV0RCxXQUFTLFdBQVcsTUFBNkI7QUFFL0MsUUFBSSxHQUFHLFdBQVcsSUFBSSxHQUFHO0FBQ3ZCLFlBQU0sT0FBTyxHQUFHLFNBQVMsSUFBSTtBQUM3QixVQUFJLEtBQUssT0FBTyxFQUFHLFFBQU87QUFFMUIsaUJBQVcsT0FBTyxZQUFZO0FBQzVCLGNBQU0sSUFBSSxLQUFLLEtBQUssTUFBTSxHQUFHO0FBQzdCLFlBQUksR0FBRyxXQUFXLENBQUMsRUFBRyxRQUFPO0FBQUEsTUFDL0I7QUFDQSxhQUFPO0FBQUEsSUFDVDtBQUVBLGVBQVcsT0FBTyxZQUFZO0FBQzVCLFlBQU0sSUFBSSxPQUFPO0FBQ2pCLFVBQUksR0FBRyxXQUFXLENBQUMsRUFBRyxRQUFPO0FBQUEsSUFDL0I7QUFDQSxXQUFPO0FBQUEsRUFDVDtBQUdBLFFBQU0sWUFBWSxLQUFLLEtBQUssUUFBUSxnQkFBZ0I7QUFDcEQsUUFBTSxZQUFvQztBQUFBLElBQ3hDLFFBQVEsS0FBSyxLQUFLLFFBQVEsaUJBQWlCO0FBQUEsSUFDM0MsTUFBTSxLQUFLLEtBQUssUUFBUSxlQUFlO0FBQUEsSUFDdkMsUUFBUSxLQUFLLEtBQUssUUFBUSxpQkFBaUI7QUFBQSxJQUMz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

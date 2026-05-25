import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import path from 'path'
import fs from 'fs'

const srcDir = path.resolve(__dirname, './src')

// 解决路径中含 # 字符导致 Rollup/Vite 扩展名解析失败的问题
function hashPathFixPlugin() {
  const extensions = ['.ts', '.tsx', '.js', '.jsx', '.json', '.css']
  const indexFiles = extensions.map(ext => 'index' + ext)

  function tryResolve(base: string): string | null {
    // 直接匹配（带扩展名）
    if (fs.existsSync(base)) {
      const stat = fs.statSync(base)
      if (stat.isFile()) return base
      // 文件夹则查 index
      for (const idx of indexFiles) {
        const p = path.join(base, idx)
        if (fs.existsSync(p)) return p
      }
      return null
    }
    // 尝试加扩展名
    for (const ext of extensions) {
      const p = base + ext
      if (fs.existsSync(p)) return p
    }
    return null
  }

  // Node.js built-in 桩映射（解决 Axios 等库在浏览器构建中的兼容问题）
  const emptyStub = path.join(srcDir, 'stubs/empty.ts')
  const nodeStubs: Record<string, string> = {
    stream: path.join(srcDir, 'stubs/stream.ts'),
    util: path.join(srcDir, 'stubs/util.ts'),
    events: path.join(srcDir, 'stubs/events.ts'),
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
    'follow-redirects': emptyStub,
  }

  return {
    name: 'hash-path-fix',
    enforce: 'pre' as const,
    // 仅在 build 模式下启用（dev 模式用 resolve.alias，不受 # 路径影响）
    apply: 'build' as const,
    resolveId(source: string, importer: string | undefined) {
      // Node built-in 桩
      if (nodeStubs[source]) return nodeStubs[source]

      // 处理 @ 别名
      if (source.startsWith('@/')) {
        const rest = source.slice(2)
        const resolved = tryResolve(path.join(srcDir, rest))
        return resolved
      }
      // 处理相对导入（仅限项目源码，不干预 node_modules 内部解析）
      if (importer && source.startsWith('.') && !importer.includes('node_modules')) {
        const importerDir = path.dirname(importer)
        const resolved = tryResolve(path.resolve(importerDir, source))
        return resolved
      }
      return null
    },
  }
}

export default defineConfig({
  base: '/spa/',
  plugins: [hashPathFixPlugin(), react()],
  resolve: {
    alias: {
      '@': srcDir,
    },
  },
  server: {
    port: 5173,
    proxy: {
      '/api': { target: 'https://localhost:7051', secure: false, changeOrigin: true },
      '/course': { target: 'https://localhost:7051', secure: false, changeOrigin: true },
      '/word': { target: 'https://localhost:7051', secure: false, changeOrigin: true },
      '/exam': { target: 'https://localhost:7051', secure: false, changeOrigin: true },
      '/examv2': { target: 'https://localhost:7051', secure: false, changeOrigin: true },
      '/hearing': { target: 'https://localhost:7051', secure: false, changeOrigin: true },
      '/whisper': { target: 'https://localhost:7051', secure: false, changeOrigin: true },
      '/login': { target: 'https://localhost:7051', secure: false, changeOrigin: true },
      '/home': { target: 'https://localhost:7051', secure: false, changeOrigin: true },
      '/statistics': { target: 'https://localhost:7051', secure: false, changeOrigin: true },
      '/translate': { target: 'https://localhost:7051', secure: false, changeOrigin: true },
      '/userinfo': { target: 'https://localhost:7051', secure: false, changeOrigin: true },
      '/import': { target: 'https://localhost:7051', secure: false, changeOrigin: true },
    },
  },
  build: {
    outDir: '../wwwroot/spa',
    emptyOutDir: true,
    rollupOptions: {
      onwarn(warning, warn) {
        if (warning.code === 'MODULE_LEVEL_DIRECTIVE') return;
        warn(warning);
      },
    },
  },
})

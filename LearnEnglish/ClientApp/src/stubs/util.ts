// 浏览器环境下 Node.js util 模块的桩
export function promisify(fn: unknown) { return fn; }
export function inherits() {}
export function deprecate(fn: unknown) { return fn; }
export default { promisify, inherits, deprecate };

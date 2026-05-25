// 浏览器环境下 Node.js events 模块的桩
export class EventEmitter {
  _listeners: Record<string, Array<(...args: unknown[]) => void>> = {};
  on(event: string, fn: (...args: unknown[]) => void) { (this._listeners[event] ??= []).push(fn); return this; }
  off(event: string, fn: (...args: unknown[]) => void) { this._listeners[event] = (this._listeners[event] ?? []).filter(f => f !== fn); return this; }
  emit(event: string, ...args: unknown[]) { (this._listeners[event] ?? []).forEach(fn => fn(...args)); return true; }
  once(event: string, fn: (...args: unknown[]) => void) {
    const wrapped = (...args: unknown[]) => { this.off(event, wrapped); fn(...args); };
    return this.on(event, wrapped);
  }
  addListener(event: string, fn: (...args: unknown[]) => void) { return this.on(event, fn); }
  removeListener(event: string, fn: (...args: unknown[]) => void) { return this.off(event, fn); }
  removeAllListeners(event?: string) { if (event) delete this._listeners[event]; else this._listeners = {}; return this; }
  listeners(event: string) { return this._listeners[event] ?? []; }
  listenerCount(event: string) { return (this._listeners[event] ?? []).length; }
}
export default EventEmitter;

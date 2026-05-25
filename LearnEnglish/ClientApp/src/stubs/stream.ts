// 浏览器环境下 Node.js stream 模块的桩
export class Readable {
  pipe() { return this; }
  on() { return this; }
  read() { return null; }
}
export class Writable extends Readable {}
export class Transform extends Readable {}
export class Duplex extends Readable {}
export class PassThrough extends Readable {}
export default { Readable, Writable, Transform, Duplex, PassThrough };

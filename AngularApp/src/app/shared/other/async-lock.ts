export class AsyncLock {
    promise: Promise<void>;
    disable: () => void;
    constructor () {
      this.disable = () => {}
      this.promise = Promise.resolve()
    }
  
    enable () {
      this.promise = new Promise(resolve => this.disable = resolve)
    }
  }
  
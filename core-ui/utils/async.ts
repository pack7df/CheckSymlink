export class AsyncRunner<T> {
    private action: (() => Promise<T>) | null = null;
    private nextCall: boolean = false;

    public run(action: () => Promise<T>) {
        if (this.action) {
            this.nextCall = true;
            this.action = action;
            return;
        } 
        this.action = action;
        return new Promise((resolve, reject) => {
            action().then(result => {
                var action = this.action;
                this.action = null;
                resolve(result);
                if (this.nextCall) {
                    this.nextCall = false;
                    this.run(action as any);
                }
            }).catch(error => {
                var action = this.action;
                this.nextCall = false;
                this.action = null;
                reject(error);
                if (this.nextCall) {
                    this.nextCall = false;
                    this.run(action as any);
                }
            });
        });
    }
}
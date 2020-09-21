import { createState, State, useState, StateMethodsDestroy } from '@hookstate/core';
import { formatMessage } from '../services/http.service';

export interface BaseState {
    isBusy: boolean;
    errorMessage?: string;
}

class Store<T extends BaseState> {
    protected _state: State<T> & StateMethodsDestroy

    public get state(): State<T> { return useState(this._state); }

    constructor(initialState?: Partial<T>) {
        this._state = createState(initialState as T)        
    }

    protected async handleCallAsync(func: () => Promise<any>) {
        return await (async () => {
            try {
                this._state.set(s => { s.errorMessage = undefined; s.isBusy = true; return s; });
                return await func();
            }
            catch (ex) {
                this._state.set(s => {
                    s.errorMessage = formatMessage(ex);
                    return s;
                });
            }
            finally {
                this._state.set(s => { s.isBusy = false; return s; });
            }
        })()
    }
}

export default Store;
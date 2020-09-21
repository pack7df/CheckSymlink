import HttpService, { formatMessage } from '../services/http.service';
import { container } from 'src/inversify.config';
import Store, { BaseState } from './store';
import { CommandResult } from './types';
import { createPatch } from 'rfc6902';

export interface FormState<T> extends BaseState {
    item: T;
}

class FormStore<TItem, TNewItem> extends Store<FormState<TItem>> {
    protected httpService!: HttpService

    protected _baseUrl: string;

    public get baseUrl() { return this._baseUrl; }
    public set baseUrl(url: string) { this._baseUrl = url; }

    constructor(baseUrl: string, initialState?: TItem) {
        super({
            errorMessage: undefined,
            isBusy: false,
            item: initialState
        });
        this.httpService = container.get(HttpService);
        this._baseUrl = baseUrl;
    }

    public async load(id: string) {
        return await this.handleCallAsync(async () => {
            var response = await this.httpService.get<TItem>(`${this.baseUrl}/${encodeURIComponent(id)}`);
            this._state.set(s => {
                s.item = response.data as TItem;
                return s;
            });
            return true;
        })
    }

    public async create(item: TNewItem) {
        return await this.handleCallAsync(async () => {
            var result = await this.httpService.post<TNewItem, CommandResult<TItem>>(`${this.baseUrl}`, item)
            return result.data
        })
    }

    public async save(id: string, item: TItem) {
        return await this.handleCallAsync(async () => {
            var result = await this.httpService.put(`${this.baseUrl}/${encodeURIComponent(id)}`, item)
            return result.data
        });
    }

    public async patch(selector: (o: TItem) => boolean, path: string, partial: Partial<TItem>) {
        return await this.handleCallAsync(async () => {
            var item = this._state.item;
            var result = await this.httpService.patch(`${this.baseUrl}/${encodeURIComponent(path)}`, createPatch(item, partial))
            return result.data
        });
    }

    public async delete(id: string, params?: any) {
        return await this.handleCallAsync(async () => {
            return await this.httpService.delete<any, CommandResult<TItem>>(`${this.baseUrl}/${encodeURIComponent(id)}`, params)
        })
    }
}

export default FormStore;
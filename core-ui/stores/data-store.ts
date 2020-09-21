import HttpService, { formatMessage } from '../services/http.service';
import { container } from 'src/inversify.config';
import Store, { BaseState } from './store';
import * as buildQuery from 'odata-query';
import { CommandResult } from './types';
import { createPatch } from 'rfc6902';

export type SortDirection = 'Ascending' | 'Descending';

export interface OrderDefinition {
    field: string;
    direction: SortDirection;
    useProfile: boolean;
}

export interface QueryParameters {
    [key: string]: string | string[] | number | number[] | undefined;
}

export interface SortProfile {
    profile: string,
    direction: SortDirection
}

export interface Query {
    searchQuery: string;
    orderBy?: OrderDefinition[];
    skip: number;
    take: number;
    parameters?: QueryParameters;
    filter?: object;
}

export interface QueryResult<T> {
    count: number;
    items: T[];
}

export interface ListState<T> extends BaseState {
    count: number;
    items: T[];
}

class DataStore<T> extends Store<ListState<T>> {
    protected httpService!: HttpService

    protected _baseUrl: string;

    public get baseUrl() { return this._baseUrl; }
    public set baseUrl(url: string) { this._baseUrl = url; }

    constructor(baseUrl: string, initialState: T[]) {
        super({
            count: initialState ? initialState.length : 0,
            items: initialState || []
        });
        this.httpService = container.get(HttpService);
        this._baseUrl = baseUrl;
    }

    public async load(query: Partial<Query>) {
        return await this.handleCallAsync(async () => {
            var response = await this.httpService.get<QueryResult<T>>(`${this.baseUrl}?${DataStore.buildUrl(query as Query)}`);
            this._state.set(s => {
                s.items = response.data.items || [];
                s.count = response.data.count || 0;
                return s;
            });
            return true;
        })
    }

    protected async save(id: string, item: T) {
        return await this.handleCallAsync(async () => {
            var result = await this.httpService.put(`${this.baseUrl}/${encodeURIComponent(id)}`, item)
            return result.data
        })
    }

    protected async patch(selector: (o: T) => boolean, path: string, partial: Partial<T>) {
        return await this.handleCallAsync(async () => {
            var items = this._state.value.items.filter(o => selector(o));
            if (items == null || items.length == 0 || items.length > 1)
                throw "No item found to patch";
            var item = items[0];
            var result = await this.httpService.patch(`${this.baseUrl}/${encodeURIComponent(path)}`, createPatch(item, partial))
            return result.data
        })
    }

    public async delete(id: string, params?: any) {
        return await this.handleCallAsync(async () => {
            var response = await this.httpService.delete<any, CommandResult<T>>(`${this.baseUrl}/${encodeURIComponent(id)}`, params)
            return response.data;
        })
    }

    private static buildUrl(query: Query) {
        const parts = [];
        if (query.searchQuery) {
            parts.push(`$search=${query.searchQuery}`);
        }

        var oDataQuery = {
            skip: query.skip || 0,
            top: query.take || 10,
        } as any;

        if (query.orderBy && query.orderBy.length > 0) {
            var sortProfile = query.orderBy.filter(o => o.useProfile);
            if (sortProfile.length > 0) {
                parts.push(`sortProfile=${sortProfile[0].field} ${sortProfile[0].direction}`);
            } else {
                var order = [];
                for (var i = 0; i < query.orderBy.length; i++) {
                    let direction = query.orderBy[i].direction == 'Ascending' ? 'asc' : 'desc';
                    order.push(`${query.orderBy[i].field} ${direction}`)
                }
                oDataQuery['orderBy'] = order;
            }
        }

        if (query.filter) {
            oDataQuery['filter'] = query.filter;
        }

        parts.push(buildQuery.default(oDataQuery).substr(1));

        if (query.parameters) {
            for (var prop in query.parameters as any) {
                if (query.parameters[prop] && query.parameters[prop]!.constructor === Array) {
                    for (var idx = 0; idx < (query.parameters[prop] as any)!.length; idx++) {
                        if ((query.parameters[prop] as any)![idx])
                            parts.push(`${prop}=${encodeURIComponent((query.parameters[prop] as any)![idx])}`)
                    }
                } else {
                    if (query.parameters[prop])
                        parts.push(`${prop}=${encodeURIComponent(query.parameters[prop] as string)}`)
                }
            }
        }
        return parts.join('&');
    }
}

export default DataStore;
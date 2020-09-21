import * as Axios from "axios";
import { inject, injectable } from "inversify";
import { stringify } from "query-string";
import { addTask } from "domain-task";
import { authenticationService } from './authentication';
import autobind from 'autobind-decorator';

@injectable()
export default class HttpService {
    protected static timeout: number = 60000;
    public serviceUrl: string = "";
    private failureCount: number = 0;
    public static accessToken: string;
    public static language: string;

    public online(): Promise<Axios.AxiosResponse> {
        return this.get("api/v1/online");
    }

    @autobind
    public async get<TResponse>(url: string, data?: any): Promise<Axios.AxiosResponse<TResponse>> {
        try {
            if (data) {
                const params = {} as any;
                for (const prop of Object.getOwnPropertyNames(data)) {
                    const type = typeof data[prop];
                    if (type !== "function" && type !== "object") {
                        params[prop] = data[prop];
                    }
                }
                url += (url.search(/\?/) !== -1 ? "&" : "?") + stringify(params);
            }
            const config = await this.getConfig(url, data);
            const response = this.httpClient(url).get(url, config);
            addTask(response);
            var result = await response;
            this.failureCount = 0;
            return result;
        } catch (reason) {
            //alert("Network error\n" + reason);
            return Promise.reject(this.handleError(reason));
        }
    }

    @autobind
    public async post<TRequest, TResponse>(url: string, data: TRequest , dataconfig? : any): Promise<Axios.AxiosResponse<TResponse>> {
        try {
            if (dataconfig) {
                const params = {} as any
                for (const prop of Object.getOwnPropertyNames(dataconfig)) {
                    const type = typeof dataconfig[prop]
                    if (type !== "function" && type !== "object") {
                        params[prop] = dataconfig[prop]
                    }
                }
                url += (url.search(/\?/) !== -1 ? "&" : "?") + stringify(params)
            }
            const config = await this.getConfig(url, dataconfig);
            const response = this.httpClient(url).post(url, data, config);
            addTask(response);
            var result = await response;
            this.failureCount = 0;
            return result;
        } catch (reason) {
            return Promise.reject(this.handleError(reason));
        }
    }

    @autobind
    public async patch<TRequest, TResponse>(url: string, data: TRequest, dataconfig?: any): Promise<Axios.AxiosResponse<TResponse>> {
        try {
            if (dataconfig) {
                const params = {} as any
                for (const prop of Object.getOwnPropertyNames(dataconfig)) {
                    const type = typeof dataconfig[prop]
                    if (type !== "function" && type !== "object") {
                        params[prop] = dataconfig[prop]
                    }
                }
                url += (url.search(/\?/) !== -1 ? "&" : "?") + stringify(params)
            }
            const config = await this.getConfig(url, dataconfig);
            const response = this.httpClient(url).patch(url, data, config);
            addTask(response);
            var result = await response;
            this.failureCount = 0;
            return result;
        } catch (reason) {
            return Promise.reject(this.handleError(reason));
        }
    }

    @autobind
    public async put<TRequest, TResponse>(url: string, data: TRequest, dataconfig?: any): Promise<Axios.AxiosResponse<TResponse>> {
        try {
            if (dataconfig) {
                const params = {} as any
                for (const prop of Object.getOwnPropertyNames(dataconfig)) {
                    const type = typeof dataconfig[prop]
                    if (type !== "function" && type !== "object") {
                        params[prop] = dataconfig[prop]
                    }
                }
                url += (url.search(/\?/) !== -1 ? "&" : "?") + stringify(params)
            }
            const config = await this.getConfig(url, dataconfig);
            const response = this.httpClient(url).put(url, data, config);
            addTask(response);
            var result = await response;
            this.failureCount = 0;
            return result;
        } catch (reason) {
            return Promise.reject(this.handleError(reason));
        }
    }

    @autobind
    public async delete<TRequest, TResponse>(url: string, data?: TRequest, dataconfig?: any): Promise<Axios.AxiosResponse<TResponse>> {
        try {
            if (dataconfig) {
                const params = {} as any
                for (const prop of Object.getOwnPropertyNames(dataconfig)) {
                    const type = typeof dataconfig[prop]
                    if (type !== "function" && type !== "object") {
                        params[prop] = dataconfig[prop]
                    }
                }
                url += (url.search(/\?/) !== -1 ? "&" : "?") + stringify(params)
            }
            const config: Axios.AxiosRequestConfig = await this.getConfig(url, dataconfig);
            config.data = data;
            const response = this.httpClient(url).delete(url, config);
            addTask(response);
            var result = await response;
            this.failureCount = 0;
            return result;
        } catch (reason) {
            return Promise.reject(this.handleError(reason));
        }
    }

    public setup(serviceUrl: string) {
        this.serviceUrl = serviceUrl;
    }

    protected handleError(error: Axios.AxiosError) {
        let msg;
        if (error.response && error.response.status) {
            switch (error.response.status) {
                case 404: {
                    msg = { message: "Not found", status: error.response.status };
                }
                case 401:
                    msg = { message: "Access is denied", status: error.response.status };
                    if (this.failureCount < 3) {
                        this.failureCount++;
                        authenticationService.loginSilentAsync();
                    }
                    break;
                default:
                case 400:
                    msg = formatMessage(error.response.data)
                    break;
            }
        } else {
            msg = error;
        }

        //alert(msg);
        return msg;
    }

    private httpClient(url: string): Axios.AxiosInstance {
        let newUrl = this.serviceUrl;
        if (url.startsWith('/')) {
            var parser = document.createElement('a');
            parser.href = this.serviceUrl;
            newUrl = `${parser.protocol}//${parser.host}`;
        } else {
            newUrl = this.serviceUrl;
        }
        return Axios.default.create({
            baseURL: newUrl,
            timeout: HttpService.timeout,
        });
    }

    private async getConfig(url: string, data?: any) {
        let headers = {} as any;
                
        var antiForgeryToken = this.getCookie("XSRF-TOKEN")
        if (antiForgeryToken) {
            headers["X-XSRF-TOKEN"] = antiForgeryToken;
        }

        if (HttpService.accessToken) {
            headers['Authorization'] = `Bearer ${HttpService.accessToken}`;
        }
        if (HttpService.language) {
            headers['language'] = HttpService.language;
        }

        return Object.assign(data || {}, {
            headers: headers
        });
    }

    private getCookie(name: string) {
        var match = document.cookie.match(new RegExp('(^| )' + name + '=([^;]+)'));
        if (match) return match[2];
    }

    //private getMeta(metaName: string) {
    //    const metas = document.getElementsByTagName('meta');

    //    for (let i = 0; i < metas.length; i++) {
    //        if (metas[i].getAttribute('name') === metaName) {
    //            return metas[i].getAttribute('content');
    //        }
    //    }

    //    return '';
    //}
}

export function formatMessage(result: any): string {
    let message: string = "Unknown error";
    if (result && result.error) {
        return result.error;
    }
    if (result && result.message) {
        if (result.message.message)
            return result.message.message;
        return result.message;
    }
    if (result && result.messages && result.messages.length > 0) {
        if (result.messages[0].body) {
            if (result.messages[0].body.message) {
                return result.messages.map((o: any) => o.body.message).join(". ");
            }
            return result.messages.map((o: any) => o.body).join(". ");
        }
        return result.messages.join(". ");
    }
    if (result && result.response && result.response.data) {
        if (result.response.data.messages) {
            message = result.response.data.messages[0].body || result.response.data.messages[0].error;
        }
        if (result.response.data.error) {
            message = `${result.response.status} ${result.response.data.error}`;
        }
        message = `${result.response.status} ${result.response.message}`;
    }
    if (typeof result === 'string' || result instanceof String)
        return result as string;
    return message;
}
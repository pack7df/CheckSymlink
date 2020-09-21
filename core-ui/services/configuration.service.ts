import { injectable } from "inversify";
import * as Axios from "axios";
import autobind from "autobind-decorator";

export interface AppConfiguration {
    serviceUrl: string;
    apps: string[],
    scopes: string,
    clientId: string,
    identityServerUrl: string
}

@injectable()
export class ConfigurationService {
    protected configuration: AppConfiguration | undefined = undefined;

    @autobind
    public current(): Promise<AppConfiguration> {
        if (this.configuration) {
            return Promise.resolve(this.configuration);
        }
        return new Promise((resolve, reject) => {
            var http = Axios.default.create({
                timeout: 30000,
            });
            http.get("/identity/api/v1/config")
                .then(result => {
                    this.configuration = <AppConfiguration>{
                        serviceUrl: result.data.serviceUrl,
                        apps: result.data.apps,
                        scopes: result.data.scopes,
                        clientId: result.data.clientId,
                        identityServerUrl: result.data.identityServerUrl
                    };
                    resolve(this.configuration);
                })
                .catch(reject);
        });
    }
}
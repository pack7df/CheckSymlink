import * as React from 'react';
import Oidc, { UserManagerSettings } from 'oidc-client';
import 'src/core/assets/loader.css';
import HttpService from './http.service';
import autobind from 'autobind-decorator';
import 'src/core/utils/linq';

interface SecureContentProps {
    config: AuthenticationConfig;
    children?: React.ReactNode;
    authenticatingView?: React.ReactNode;
    onAuthentication?: (result: AuthenticationResult) => void;
}

interface SecureContentStatus {
    authenticating: boolean,
    result: AuthenticationResult | undefined,
    error: string | undefined
}

function AuthenticatingView(props: any) {
    return <div className="loaderContainer">
        <div className="loader">
            <div className="circle">&nbsp;</div>
            <div className="circle">&nbsp;</div>
            <div className="circle">&nbsp;</div>
            <div className="circle">&nbsp;</div>
        </div>
    </div>;
}

interface AuthenticationConfig {
    clientId: string,
    authority?: string,
    resource?: string,
    redirectUri?: string,
    passthrough?: string, 
    extraQueryParameter?: string
}

export interface AuthenticationResult {
    idToken: string | undefined;
    accessToken: string | undefined;
    firstName: string | undefined;
    lastName: string | undefined;
    email: string | undefined;
    roles: string[] | undefined;
    scopes: string[] | undefined;
    groups: string[] | undefined;
    profile: any | undefined;
    error: string | undefined;
    logout: () => Promise<any>;
}

function ensureArray(item: any): any {
    if (Array.isArray(item))
        return item;
    if (!item)
        return [];
    return [item];
}

class AuthenticationService {
    private config: AuthenticationConfig;
    private _identityToken: undefined | string;
    private _accessToken: undefined | string;
    private _user: undefined | string;
    private _authenticationContext: Oidc.UserManager | undefined = undefined;

    public get identityToken(): string | undefined {
        return this._identityToken;
    }

    public get accessToken(): string | undefined {
        return this._accessToken;
    }

    public get user(): string | undefined {
        return this._user;
    }

    private get context() {
        this._identityToken = undefined;
        
        this.loginAsync = this.loginAsync.bind(this);
        this.logout = this.logout.bind(this);
        this.processResult = this.processResult.bind(this);        
        return this._authenticationContext = new Oidc.UserManager({
            automaticSilentRenew: true,
            authority: this.config.authority,
            client_id: this.config.clientId,
            redirect_uri: this.config.redirectUri,
            response_type: "id_token token",
            scope: `${this.config.resource}`,
            post_logout_redirect_uri: this.config.redirectUri,
            silent_redirect_uri: this.config.redirectUri,
            loadUserInfo: true,
            userStore: new Oidc.WebStorageStateStore({ store: window.localStorage })
        });
    }

    constructor(config: AuthenticationConfig) {
        this.config = config;
    }

    public logout() {
        return this.context.signoutRedirect();
    }

    public loginSilentAsync(): Promise<AuthenticationResult> {
        if (this.config.passthrough) {
            return Promise.resolve(this.processResult({ id_token: 'debug', access_token: 'debug', profile: this.config.passthrough }));
        }
        return new Promise((resolve, reject) => {
            var self = this;
            var mgr = this.context;
            mgr.signinSilentCallback().then(function () {
                mgr.getUser().then(function (user) {
                    if (user && !user.expired && self.scopesAreValid(user.scopes)) {
                        console.log("User logged in", user.profile);
                        resolve(self.processResult(user));
                    }
                    else {
                        console.log("User not logged in");
                        mgr.signinSilent();
                    }
                });
            }).catch(function (e) {
                mgr.getUser().then(function (user) {
                    if (user && !user.expired && self.scopesAreValid(user.scopes)) {
                        console.log("User logged in", user.profile);
                        resolve(self.processResult(user));
                    }
                    else {
                        console.log("User not logged in");
                        mgr.signinSilent();
                    }
                });
            });
        });
    }

    @autobind
    private scopesAreValid(userScopes: string[]): boolean {
        var requestedScopes = this.config!.resource!.split(" ");
        return requestedScopes.all(scope => userScopes.any(o => o == scope));
    }

    public loginAsync(): Promise<AuthenticationResult> {
        if (this.config.passthrough) {
            return Promise.resolve(this.processResult({ id_token: 'debug', access_token: 'debug', profile: this.config.passthrough }));
        }
        return new Promise((resolve, reject) => {
            var self = this;
            var mgr = this.context;
            mgr.events.addUserSignedOut(function () {
                mgr.removeUser().then(result=>mgr.signinSilent());
            });
            mgr.signinRedirectCallback().then(function () {
                mgr.getUser().then(function (user) {
                    if (user && !user.expired && self.scopesAreValid(user.scopes)) {
                        console.log("User logged in", user.profile);
                        resolve(self.processResult(user));
                    }
                    else {
                        console.log("User not logged in");
                        mgr.signinRedirect();
                    }
                });
            }).catch(function (e) {
                mgr.getUser().then(function (user) {
                    if (user && !user.expired && self.scopesAreValid(user.scopes)) {
                        console.log("User logged in", user.profile);
                        resolve(self.processResult(user));
                    }
                    else {
                        console.log("User not logged in");
                        mgr.signinRedirect();
                    }
                });
            });
        });
    }

    private processResult(result: any): AuthenticationResult {
        this._identityToken = result.id_token;
        this._accessToken = result.access_token;
        this._user = result.profile;
        var profile = result.profile || {}
        HttpService.accessToken = result.access_token;
        return {
            accessToken: result.access_token,
            idToken: result.id_token,
            profile: result.profile,
            error: undefined,
            firstName: profile["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname"] || profile["given_name"],
            lastName: profile["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname"] || profile["family_name"],
            email: profile["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"] || profile["email"],
            roles: ensureArray(profile["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"]) || ensureArray(profile["role"]),
            scopes: ensureArray(profile["scope"]),
            groups: ensureArray(profile["group"]),
            logout: this.logout
        }
    }
}

let authenticationService: AuthenticationService;

const { Provider, Consumer } = React.createContext<AuthenticationResult>(undefined as any);

class SecureContent extends React.Component<SecureContentProps, SecureContentStatus> {
    private authenticationService: AuthenticationService;

    constructor(props: SecureContentProps) {
        super(props);
        this.authenticationService = authenticationService = new AuthenticationService(props.config);
        this.state = {
            result: undefined,
            authenticating: true,
            error: undefined
        }
		
        this.authenticate = this.authenticate.bind(this);
    }

    componentWillMount() {
        this.authenticate().then(result => {
            this.setState({ authenticating: false, error: result.error });
            if (this.props.onAuthentication) {
                this.props.onAuthentication(result);
            }
        }).catch(error => {
            this.setState({ error: error });
        });
    }

    private authenticate(): Promise<AuthenticationResult> {
        return new Promise<AuthenticationResult>((resolve, reject) => {
            this.authenticationService.loginAsync().then(result => {
                this.setState({ result: result });
                resolve(result);
            }).catch(error => {
                resolve({
                    idToken: undefined,
                    accessToken: undefined,
                    profile: undefined,
                    error: error,
                    email: undefined,
                    firstName: undefined,
                    lastName: undefined,
                    roles: undefined,
                    groups: undefined,
                    scopes: undefined,
                    logout: this.authenticationService.logout
                });
            });
        });
    }

    public render() {
        if (this.state.error) {
            return <div>
                <h1>ERROR</h1>
                <p>{JSON.stringify(this.state.error)}</p>
            </div>;
        }
        return (this.state.authenticating) ? (this.props.authenticatingView || <AuthenticatingView />) : (<Provider value={this.state.result as any}>{this.props.children}</Provider> || <div></div>);
    }
}

export interface IdentityProps {
    identity: AuthenticationResult
}

export function withIdentity<T extends React.ComponentType<any>>(Component: T) {
    // ...and returns another component...
    return function ComponentBoundWithAppContext(props: any) {
        // ... and renders the wrapped component with the current context!
        // Notice that we pass through any additional props as well
        return (
            <Consumer>
                {appContext => <Component {...props} identity={appContext} />}
            </Consumer>
        );
    };
}

export function isInRole(identity: AuthenticationResult, roles: string[]) {
    if (!identity || !identity.roles)
        return false;
    let exists = false;
    identity.roles.map(role => exists = exists || roles.filter(r => r == role).length > 0);
    return exists;
}

export { authenticationService };
export default SecureContent;
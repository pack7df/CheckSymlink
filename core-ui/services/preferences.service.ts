import { injectable } from "inversify";
import autobind from "autobind-decorator";
import HttpService from './http.service';
import { authenticationService } from './authentication';

interface CompanyReference {
    id: string,
    title: string,
}

interface Preferences {
    currentLanguage: string,
    currentCompany: CompanyReference | undefined;
}

const PREFERENCES_KEY = "FI_Preferences";
@injectable()
export class PreferencesService {
    protected preferences: Preferences | undefined;

    private readonly languages = [
        { key: 'en', value: 'English' },
        { key: 'es', value: 'Spanish' },
        { key: 'fr', value: 'French' }
    ]

    public constructor() {
        this.preferences = undefined;
    }

    @autobind
    public current(): Promise<Preferences> {
        if (this.preferences) {
            return Promise.resolve(this.preferences);
        }
        var rawPreferences = window.localStorage.getItem(PREFERENCES_KEY);
        if (rawPreferences) {
            this.preferences = JSON.parse(rawPreferences as string) as Preferences;
        } else {
            this.preferences = {
                currentCompany: undefined,
                currentLanguage: this.getBrowserLanguage()
            };
        }
        return Promise.resolve(this.preferences);
    }

    @autobind
    public async setCurrentCompany(companyId: string) {
        var preferences = await this.current();
        preferences.currentCompany = { id: companyId, title: "" };
        await this.savePreferences(preferences);
        return preferences;
    }

    @autobind
    public async setCurrentLanguage(language: string) {
        var preferences = await this.current();
        preferences.currentLanguage = language;
        await this.savePreferences(preferences);
        return preferences;
    }

    @autobind
    private async savePreferences(preferences: Preferences) {
        window.localStorage.setItem(PREFERENCES_KEY, JSON.stringify(this.preferences));
        return preferences;
    }

    @autobind
    private getBrowserLanguage() {
        var supportedLangs = this.languages.map(o => o.key);
        var language: string = window.navigator.language || (window.navigator as any).userLanguage || supportedLangs[0];
        supportedLangs.forEach(lang => {
            if (language && language == lang || language.startsWith(lang))
                language = lang;
        });
        return language || supportedLangs[0];
    }
}
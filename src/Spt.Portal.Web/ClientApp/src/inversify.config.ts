import 'reflect-metadata';
import * as React from 'react';
import { Container } from 'inversify';
import HttpService from 'src/core/services/http.service';
import { AppConfiguration, ConfigurationService } from "src/core/services/configuration.service";
import { PreferencesService } from "src/core/services/preferences.service";
import { ProjectItemStore, ProjectSummaryStore } from './stores/projects-store';

// Initialize DI/IoC container
const container = new Container();

const AppConfig = React.createContext<AppConfiguration>({} as any);

function initialize(config?: any) {
    let baseUri = `${config.identityServerUrl}`;
    if (!container.isBound(HttpService)) {
        // Initialize services if container is not configured before
        container.bind(HttpService).toSelf().inSingletonScope().onActivation((context: any, instance: any) => {
            instance.setup(baseUri);
            return instance;
        });
    }

    // Initialize services
    container.bind(ConfigurationService).toSelf().inSingletonScope().onActivation((context, instance) => {
        var config = instance.current();
        return instance;
    });

    container.bind(PreferencesService).toSelf().inSingletonScope().onActivation((context, instance) => {
        var prefs = instance.current();
        return instance;
    });

    container.bind(ProjectSummaryStore).toConstantValue(new ProjectSummaryStore(baseUri));
    container.bind(ProjectItemStore).toConstantValue(new ProjectItemStore(baseUri));
}

export { container, initialize, AppConfig };
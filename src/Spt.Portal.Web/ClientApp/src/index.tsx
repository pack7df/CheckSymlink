/// <reference path="modules.d.ts" />
import 'reflect-metadata';
import React from 'react';
import ReactDOM from 'react-dom';
import { BrowserRouter } from 'react-router-dom';
import App from './pages/index';
import './core/assets/site.less';
import './assets/app.less';
import { AppConfig, initialize as initializeInversify } from './inversify.config';
import 'src/core/utils/linq';
import { I18nextProvider } from 'react-i18next';
import i18n from './i18n';
import './core/utils/linq';

//import registerServiceWorker from './registerServiceWorker';

const baseUrl = document.getElementsByTagName('base')[0].getAttribute('href');
const rootElement = document.getElementById('root');

fetch(`${process.env.PUBLIC_URL}/api/config`)
    .then(response => response.json())
    .then(config => {
        initializeInversify(config);
        ReactDOM.render(
            <BrowserRouter basename={baseUrl as string}>
                <AppConfig.Provider value={config}>
                    <I18nextProvider i18n={i18n}>
                        <App />
                    </I18nextProvider>
                </AppConfig.Provider>
            </BrowserRouter>,
            rootElement);
    })
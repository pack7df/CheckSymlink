import i18next from 'i18next';
import lang from "./i18n.generated";

i18next
    .init({
        lng: 'fr',
        // we init with resources
        resources: {
            en: {
                translations: lang.data.en
            },
            es: {
                translations: lang.data.es
            },
            fr: {
                translations: lang.data.fr
            }           
        },
        fallbackLng: lang.source,
        debug: false,

        // have a common namespace used around the full app
        ns: ['translations'],
        defaultNS: 'translations',

        keySeparator: false, // we use content as keys

        interpolation: {
            escapeValue: false, // not needed for react!!
            formatSeparator: ','
        },

        react: {
            wait: true
        }
    });

export default i18next;

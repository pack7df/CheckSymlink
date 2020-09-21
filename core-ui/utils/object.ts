import moment from 'moment';

export const nameof = <T>(name: keyof T) => name;

export function isNullOrWhitespace(str: string) {
    return str === null || str.match(/^ *$/) !== null;
}

export function validateEmail(email: string): boolean {
    if (/^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,5})+$/.test(email)) {
        return true
    }
    return false
}

export function unixEpochToDate(epochTime: number): Date {
    return new Date(epochTime * 1000);
}

export const clone = <T>(object: any): T => {
    //if (typeof object === 'object')
    //    return Object.assign({}, object) as T;
    //return object as T;

    var copy;

    // Handle the 3 simple types, and null or undefined
    if (null == object || "object" != typeof object) return object;

    // Handle Date
    if (object instanceof Date) {
        copy = new Date();
        copy.setTime(object.getTime());
        return copy as any;
    }

    // Handle Array
    if (object instanceof Array) {
        copy = [];
        for (var i = 0, len = object.length; i < len; i++) {
            copy[i] = clone(object[i]);
        }
        return copy as any;
    }

    // Handle Object
    if (object instanceof Object) {
        copy = {};
        for (var attr in object) {
            if (object.hasOwnProperty(attr)) (copy as any)[attr] = clone(object[attr]);
        }
        return copy as any;
    }

    throw new Error("Unable to copy obj! Its type isn't supported.");
}

export const formatDate = (date: Date, t?: (text: string) => string, format?: string) => {
    if (!t)
        t = o => o;
    if (!format)
        format = "DD-MM-YYYY";
    if (!date)
        return `(${t("Not set")})`;
    return moment(date).format(format);
}

export const formatDateTime = (date: Date, t?: (text: string) => string, format?: string) => {
    if (!t)
        t = o => o;
    if (!format)
        format = "DD-MM-YYYY hh:mm:ss";
    if (!date)
        return `(${t("Not set")})`;
    return moment(date).format(format);
}

export const formatDateUtc = (date: Date, t?: (text: string) => string, format?: string) => {
    if (!t)
        t = o => o;
    if (!format)
        format = "DD-MM-YYYY";
    if (!date)
        return `(${t("Not set")})`;
    return moment(date).utc().format(format);
}

export const formatDecimal = (number: number, options? : any) => {
    return (number ? number : 0).toFixed(2).replace(/\./g, ',').replace(/\B(?=(\d{3})+(?!\d))/g, '.');
}

export const formatCurrency = (number: number, symbol?: string) => {
    if (!symbol)
        symbol = '€';
    return `${formatDecimal(number, {minimumFractionDigits: 2, maximumFractionDigits:2})} ${symbol}`;
}

export const formatPercent = (number: number) => {
    return `${formatDecimal(number)}%`;
}

export const formatBoolean = (value: boolean, t: (text: string) => string) => {
    return value ? t("Yes") : t("No");
}

export const getProperties = (o: any): { key: string, value: any }[] => {
    const result = [];
    for (const key in o) {
        if (o.hasOwnProperty(key)) {
            result.push({ key, value: o[key] });
        }
    }

    return result;
};

export function delay(ms: number) {
    return new Promise<void>(function (resolve) {
        setTimeout(resolve, ms);
    });
}

export function lightOrDark(color: any) {

    // Variables for red, green, blue values
    var r, g, b, hsp;

    // Check the format of the color, HEX or RGB?
    if (color.match(/^rgb/)) {

        // If HEX --> store the red, green, blue values in separate variables
        color = color.match(/^rgba?\((\d+),\s*(\d+),\s*(\d+)(?:,\s*(\d+(?:\.\d+)?))?\)$/);

        r = color[1];
        g = color[2];
        b = color[3];
    }
    else {

        // If RGB --> Convert it to HEX: http://gist.github.com/983661
        color = +("0x" + color.slice(1).replace(
            color.length < 5 && /./g, '$&$&'));

        r = color >> 16;
        g = color >> 8 & 255;
        b = color & 255;
    }

    // HSP (Highly Sensitive Poo) equation from http://alienryderflex.com/hsp.html
    hsp = Math.sqrt(
        0.299 * (r * r) +
        0.587 * (g * g) +
        0.114 * (b * b)
    );

    // Using the HSP value, determine whether the color is light or dark
    if (hsp > 127.5) {

        return 'light';
    }
    else {

        return 'dark';
    }
}
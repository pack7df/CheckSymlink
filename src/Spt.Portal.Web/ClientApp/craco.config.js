const CracoLessPlugin = require('craco-less');
const path = require('path');
//const AntdDayjsWebpackPlugin = require('antd-dayjs-webpack-plugin');
const CompressionWebpackPlugin = require('compression-webpack-plugin');

const WebpackPlugins = {
    overrideWebpackConfig: ({
        context,
        webpackConfig,
        pluginOptions,
    }) => {
        webpackConfig.resolve.symlinks = false;
        webpackConfig.resolve.alias["src"] = path.resolve('./src');
        webpackConfig.plugins.push(new CompressionWebpackPlugin({
            test: /\.(js|css|html|svg)$/,
            compressionOptions: {
                // zlib’s `level` option matches Brotli’s `BROTLI_PARAM_QUALITY` option.
                level: 9,
            },
            threshold: 10240,
            minRatio: 0.8,
            deleteOriginalAssets: false,
        }));
        //webpackConfig.plugins.push(new AntdDayjsWebpackPlugin());
        return CracoLessPlugin.overrideWebpackConfig({
            context,
            webpackConfig,
            pluginOptions,
        });
    },
};

module.exports = {
    plugins: [
        {
            plugin: CracoLessPlugin,
            options: {
                lessLoaderOptions: {
                    lessOptions: {
                        modifyVars: {
                            //'@primary-color': '#1DA57A',
                            //'@primary-color': '#1890ff',
                            //'@link-color': '#1890ff',
                            //'@success-color': '#52c41a',
                            //'@warning-color': '#faad14',
                            //'@error-color': '#f5222d',
                            //'@font-size-base': '14px;',
                            //'@heading-color': 'rgba(0, 0, 0, .85)',
                            //'@text-color': 'rgba(0, 0, 0, .65)',
                            //'@text-color-secondary': 'rgba(0, 0, 0, .45)',
                            //'@disabled-color': 'rgba(0, 0, 0, .25)',
                            //'@border-radius-base': '2px;',
                            //'@border-color-base': '#d9d9d9',
                            //'@input-height-base': '36px',
                            //'@font-family': 'FIFont, Open Sans, "Lato", -apple-system, BlinkMacSystemFont, "Segoe UI", "Helvetica Neue", Helvetica, Arial, sans-serif, "Apple Color Emoji", "Segoe UI Emoji", "Segoe UI Symbol"'
                        },
                        javascriptEnabled: true,
                    },
                },
            },
        },
        { plugin: WebpackPlugins }
    ]
};
const path = require('path');
const CopyPlugin = require('copy-webpack-plugin');

module.exports = {
    entry: {
        app: path.join(__dirname, '../src/App/App.fsproj'),
        options: path.join(__dirname, '../src/Options/Options.fsproj'),
    },
    output: {
        path: path.join(__dirname, '../dist/js'),
        filename: '[name].js',
    },
    optimization: {
        splitChunks: {
            name: 'vendor',
            chunks: 'initial',
        },
    },
    module: {
        rules: [
            {
                test: /\.fs(x|proj)?$/,
                use: 'fable-loader',
            },
        ],
    },
    plugins: [
        new CopyPlugin({
            patterns: [{ from: path.resolve(__dirname, '../public'), to: path.resolve(__dirname, '../dist') }],
        }),
    ],
};

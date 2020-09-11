const merge = require('webpack-merge');
const config = require('./webpack.config');

module.exports = merge.merge(config, {
    mode: 'production',
});

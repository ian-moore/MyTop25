var path = require('path'),
    webpack = require('webpack');

module.exports = {
    devtool: 'source-map',
    entry: './js/app.jsx',
    output: {
        path: path.join(__dirname, 'content'),
        filename: 'app.js'
    },
    module: {
        loaders: [
            {
                test: /\.jsx$/,
                exclude: /node_modules/,
                loader: 'babel-loader',
                query: {
                    presets: ['es2015', 'react']
                }
            }
        ]
    }
};
import { AureliaPlugin, ModuleDependenciesPlugin } from "aurelia-webpack-plugin";
// tslint:disable:import-name
// tslint:disable:max-func-body-length
import ExtractTextPlugin from "extract-text-webpack-plugin";
import HtmlWebpackPlugin from "html-webpack-plugin";
import * as path from "path";
import * as webpack from "webpack";

const title = "Nimator";
const outDir: string = path.resolve(__dirname, "dist");
const srcDir: string = path.resolve(__dirname, "src");
const nodeModulesDir: string = path.resolve(__dirname, "node_modules");

interface IEnv {
  server?: boolean;
  production?: boolean;
}

const baseUrl: string = "/";
const config = (env: IEnv = {}): webpack.Configuration => {
  return {
    mode: env.production ? "production" : "development",
    resolve: {
      extensions: [".ts", ".js"],
      modules: ["src", "node_modules"],
      alias: {
        bluebird: path.join(nodeModulesDir, "bluebird/js/browser/bluebird.min"),
        jquery: path.join(nodeModulesDir, "jquery/dist/jquery.min")
      }
    },
    entry: {
      app: ["aurelia-bootstrapper"],
      vendor: ["bluebird"]
    },
    output: {
      path: outDir,
      publicPath: baseUrl,
      filename: env.production ? "[name].[chunkhash].bundle.js" : "[name].[hash].bundle.js",
      sourceMapFilename: env.production ? "[name].[chunkhash].bundle.map" : "[name].[hash].bundle.map",
      chunkFilename: env.production ? "[name].[chunkhash].chunk.js" : "[name].[hash].chunk.js"
    },
    devtool: env.production ? "nosources-source-map" : "cheap-module-eval-source-map",
    devServer: {
      contentBase: outDir,
      historyApiFallback: true
    },
    module: {
      rules: [
        {
          test: /\.css$/i,
          use: [{ loader: "style-loader" }, { loader: "css-loader" }],
          issuer: [{ not: [{ test: /\.html$/i }] }]
        },
        {
          test: /\.css$/i,
          use: [{ loader: "css-loader" }],
          issuer: [{ test: /\.html$/i }]
        },
        {
          test: /\.scss$/,
          use: [{ loader: "style-loader" }, { loader: "css-loader" }, { loader: "sass-loader" }],
          issuer: /\.[tj]s$/i
        },
        {
          test: /\.scss$/,
          use: [{ loader: "css-loader" }, { loader: "sass-loader" }],
          issuer: /\.html?$/i
        },
        {
          test: /\.(png|jpg|jpeg|gif|svg)$/,
          use: [{ loader: "file-loader" }]
        },
        {
          test: /\.(woff|woff2|ttf|svg|eot)$/,
          use: [
            {
              loader: "url-loader",
              options: {
                limit: 10240,
                name: "fonts/[name]-[hash:7].[ext]"
              }
            }
          ],
          include: [path.join(__dirname, "src")]
        },
        {
          test: /\.json$/i,
          use: [{ loader: "json-loader" }]
        },
        {
          test: /\.html$/,
          use: [{ loader: "html-loader" }]
        },
        {
          test: /\.ts$/,
          loader: "ts-loader",
          exclude: /node_modules/,
          options: {
            configFile: path.resolve(__dirname, "tsconfig.json")
          }
        },
        {
          test: /[\/\\]node_modules[\/\\]bluebird[\/\\].+\.js$/,
          use: [{ loader: "expose-loader?Promise" }]
        },
        {
          test: /[\/\\]node_modules[\/\\]jquery[\/\\].+\.js$/,
          use: [{ loader: "expose-loader?jQuery" }]
        },
        {
          test: /[\/\\]node_modules[\/\\]jquery[\/\\].+\.js$/,
          use: [{ loader: "expose-loader?$" }]
        }
      ]
    },
    plugins: [
      new AureliaPlugin(),
      new webpack.ProvidePlugin({
        Promise: "bluebird",
        jQuery: "jquery",
        $: "jquery"
      }),
      new ModuleDependenciesPlugin({
        "aurelia-testing": ["./compile-spy", "./view-spy"]
      }),
      new HtmlWebpackPlugin({
        template: "index.ejs",
        metadata: {
          title,
          server: env.server,
          baseUrl
        }
      })
    ]
  };
};

export default config;

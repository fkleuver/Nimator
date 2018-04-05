const { crossEnv } = require("nps-utils");

function package(script) {
  return `./node_modules/.bin/${script}`;
}

function webpack(tool, arg) {
  return crossEnv(`TS_NODE_PROJECT=\"tsconfig-webpack.json\" ${tool} --config webpack.config.ts ${arg}`);
}

module.exports = {
  scripts: {
    build: {
      default: "nps build.development",
      development: {
        default: webpack(package("webpack-dev-server"), "--hot --env.server")
      },
      production: {
        default: webpack(package("webpack"), "--env.production")
      }
    }
  }
};

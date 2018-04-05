import { Aurelia } from "aurelia-framework";
import environment from "./environment";
import { PLATFORM } from "aurelia-pal";
import * as Bluebird from "bluebird";

Promise.config({ warnings: { wForgottenReturn: false } });

export async function configure(au: Aurelia): Promise<void> {
  au.use.developmentLogging();
  au.use.standardConfiguration();
  au.use.plugin(PLATFORM.moduleName("aurelia-router-metadata"));
  au.use.feature(PLATFORM.moduleName("plugins/index"));
  au.use.feature(PLATFORM.moduleName("resources/index"));

  const host = document.querySelector("[aurelia-app]");
  await au.start();
  await au.setRoot(PLATFORM.moduleName("shell/app"), host);
}

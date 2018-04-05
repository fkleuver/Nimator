import { FrameworkConfiguration } from "aurelia-framework";
import { PLATFORM } from "aurelia-pal";

export function configure(fxconfig: FrameworkConfiguration): void {
  fxconfig.feature(PLATFORM.moduleName("plugins/aurelia/index"));
  fxconfig.feature(PLATFORM.moduleName("plugins/signalr/index"));
}

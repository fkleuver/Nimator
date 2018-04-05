import { FrameworkConfiguration } from "aurelia-framework";
import { PLATFORM } from "aurelia-pal";

export function configure(fxconfig: FrameworkConfiguration): void {
  fxconfig.feature(PLATFORM.moduleName("resources/attributes/index"));
  fxconfig.feature(PLATFORM.moduleName("resources/converters/index"));
  fxconfig.feature(PLATFORM.moduleName("resources/elements/index"));
}

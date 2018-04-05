import { FrameworkConfiguration } from "aurelia-framework";
import * as logging from "./logging";

export function configure(fxconfig: FrameworkConfiguration): void {
  logging.configure();
}

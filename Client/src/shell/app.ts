import { autoinject } from "aurelia-framework";
import { SignalrHubProxy } from "../plugins/signalr";
import { getLogger } from "aurelia-logging";
import { IHealthCheckResult } from "../models";

const logger = getLogger("app");

@autoinject()
export class App {
  public results: IHealthCheckResult[] = [];

  constructor(private hub: SignalrHubProxy) {
    this.hub.start();
    this.hub.on("healthCheckResult", (result: IHealthCheckResult) => {
      logger.info("health check: ", result);
      this.results.unshift(result);
    });
  }

  message = "Hello World!";
}

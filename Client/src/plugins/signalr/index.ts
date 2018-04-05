import { SignalrHubProxy } from "./signalr-hub-proxy";
import { FrameworkConfiguration } from "aurelia-framework";

export function configure(fxconfig: FrameworkConfiguration): void {
  const baseUrl = "http://localhost:8085/signalr";
  const hubName = "healthCheckResultsHub";
  const hubOpts: SignalR.Hub.Options = {};
  const connOpts: SignalR.ConnectionOptions = {};
  const signalrProxy = new SignalrHubProxy(baseUrl, hubName, hubOpts, connOpts);
  fxconfig.instance(SignalrHubProxy, signalrProxy);
}

export * from "./signalr-hub-proxy";

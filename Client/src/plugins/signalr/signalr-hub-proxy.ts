import "signalr";
import { getLogger, Logger } from "aurelia-logging";
import { TaskQueue } from "aurelia-task-queue";

export class SignalrHubProxy {
  private connection: SignalR.Hub.Connection;
  private proxy: SignalR.Hub.Proxy;

  constructor(
    public serverUrl: string,
    public hubName: string,
    public hubOptions: SignalR.Hub.Options,
    public connectionOptions: SignalR.ConnectionOptions
  ) {
    this.connection = jQuery.hubConnection(this.serverUrl, this.hubOptions);
    this.proxy = this.connection.createHubProxy(this.hubName);
  }

  public get running() {
    return this.connection;
  }

  public stop(): void {
    this.connection.stop(true, true);
  }

  public start(): void {
    this.connection
      .start(this.connectionOptions)
      .then(() => {
        if (this.hubOptions.logging) {
          this.logger.debug("Hub started:" + this.hubName);
        }
      })
      .fail(e => {
        this.logger.error(e);
      });
  }

  public on(eventName: string, callback: (result: any) => any): void {
    this.proxy.on(eventName, result => {
      if (this.hubOptions.logging) {
        this.logger.info(result);
      }
      if (callback) {
        callback(result);
      }
    });
  }

  public off(eventName: string, callback: (result: any) => any): void {
    this.proxy.off(eventName, result => {
      if (callback) {
        callback(result);
      }
    });
  }

  public invoke(methodName: string, callback: (result: any) => any): void {
    this.proxy.invoke(methodName).then(result => {
      if (callback) {
        callback(result);
      }
    });
  }

  private get logger(): Logger {
    return getLogger("bt:signalr-hub-proxy");
  }
}

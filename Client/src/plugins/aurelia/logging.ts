import { getLogger } from "aurelia-logging";
import { PLATFORM } from "aurelia-pal";

export function configure(): void {
  const global = PLATFORM.global as Window;

  global.addEventListener("error", (ev: ErrorEvent) => {
    ev.preventDefault();

    const logger = getLogger("window.onerror");
    logger.error(ev.error);
    alert(`Unhandled global error: ${ev.message}. See the browser console for more information.`);

    return false;
  });

  global.addEventListener("unhandledrejection", (ev: CustomEvent) => {
    ev.preventDefault();

    const rev = ev.detail as PromiseRejectionEvent;
    const reason = rev.reason as string | { message: string };
    let err: { message: string };
    if (
      reason === undefined ||
      reason === null ||
      typeof reason === "string" ||
      Object.prototype.toString.call(reason) === "[object String]" ||
      typeof reason !== "object"
    ) {
      err = { message: `${reason}` };
    } else {
      err = reason;
    }

    const logger = getLogger("window.onunhandledrejection");
    const message = `
      Message: ${err.message},
      Error object: ${JSON.stringify(err)}`;

    logger.error(message, ev);
    alert(`Unhandled global rejection: ${err.message}. See the browser console for more information.`);

    return false;
  });
}

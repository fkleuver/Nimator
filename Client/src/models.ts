export interface IHealthCheckResult {
  checkId: IIdentity;
  status: string;
  level: string;
  details: { [key: string]: any };
  innerResults: IHealthCheckResult[];
  reason: string;
  allReasons: {Key: string; Value: string}[];
}

export interface IIdentity {
  name: string;
}


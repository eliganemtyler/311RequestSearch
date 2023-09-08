import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { IAppLauncherOption } from '@tylertech/forge-internal';


@Injectable()
export class AppLauncherService {

  private _intentsUrl: string;

  constructor(private http: HttpClient ) {
    this._intentsUrl = 'api/applauncher';
  }

  getIntents() {
    return this.http.get<IAppLauncherOption[]>(this._intentsUrl);
  }
}

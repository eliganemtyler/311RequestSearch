import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { shareReplay } from 'rxjs/operators';
import { IPortalConfig } from '../models/config';

@Injectable({
  providedIn: 'root'
})
export class PortalConfigurationService {

  private _portalConfig$?: Observable<IPortalConfig>;
  private portalConfigUri = 'api/PortalConfiguration';

  constructor(private http: HttpClient) {
    this._setPortalConfiguration();
  }

  get portalConfig$(): Observable<IPortalConfig> | undefined {
    return this._portalConfig$;
  }

  private _setPortalConfiguration(): void {
    this._portalConfig$ = this.http.get<IPortalConfig>(this.portalConfigUri).pipe(shareReplay(1));
  }
}

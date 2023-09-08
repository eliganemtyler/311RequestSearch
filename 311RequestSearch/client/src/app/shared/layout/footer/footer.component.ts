import { Component, OnInit } from '@angular/core';
import {  IPortalConfig } from '../../../core/models/config';
import { Observable } from 'rxjs';
import { PortalConfigurationService } from 'src/app/core/services/portal-configuration.service';

@Component({
  selector: 'tyl-footer',
  styleUrls: ['./footer.component.scss'],
  templateUrl: './footer.component.html'
})
export class FooterComponent implements OnInit {

  public portalConfig$?: Observable<IPortalConfig>;

  public constructor(
    private _portalConfigService: PortalConfigurationService
  ) {
  }

  public ngOnInit(): void {
    this.portalConfig$ = this._portalConfigService.portalConfig$;
  }
  public openCookieSettingsDrawer(): void {
    (window as any).OneTrust.ToggleInfoDisplay();
  }
}

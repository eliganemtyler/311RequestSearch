import { Component, Inject, OnInit } from '@angular/core';
import { Config } from '../../core/models/config';
import { APP_CONFIG } from '../../app.tokens';

@Component({
  selector: 'tyl-app-config',
  templateUrl: './app-config.component.html',
  styleUrls: ['./app-config.component.scss']
})
export class AppConfigComponent implements OnInit {
  public brandingEndPoint = '';
  public configJson = '';

  constructor(@Inject(APP_CONFIG) private readonly config: Config) {
    this.configJson = JSON.stringify(config, undefined, 2);
  }

  public ngOnInit(): void {
    this.brandingEndPoint = this.config.brandingServiceUri;
  }
}

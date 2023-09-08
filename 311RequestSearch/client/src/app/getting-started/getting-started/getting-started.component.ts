import { Component, OnInit, Inject } from '@angular/core';
import { GettingStartedService } from './getting-started.service';
import { APP_CONFIG } from 'src/app/app.tokens';
import { Config } from 'src/app/core/models/config';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'tyl-getting-started',
  templateUrl: './getting-started.component.html',
  styleUrls: ['./getting-started.component.scss']
})
export class GettingStartedComponent implements OnInit {
  applicationName: string | null = 'Log in to see app name';
  brandingEndPoint = '';

  constructor(private gettingStartedService: GettingStartedService, @Inject(APP_CONFIG) private readonly config: Config) { }

  ngOnInit() {
    this.brandingEndPoint = this.config.brandingServiceUri;
    this.gettingStartedService.getAppName().pipe(filter(name => !!name))
      .subscribe(applicationName => this.applicationName = applicationName);
  }
}

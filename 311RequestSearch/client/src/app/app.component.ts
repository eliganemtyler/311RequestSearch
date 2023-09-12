import { Component } from '@angular/core';
import { AppService } from './app.service';
import { ActivatedRoute, ActivatedRouteSnapshot, NavigationEnd, NavigationStart, Router } from '@angular/router';
import { filter } from 'rxjs'

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  title = 'client';
  constructor(
    private router: Router,
    private route: ActivatedRoute,
    public appService: AppService
  ) {
    this.initRouteWatch();
  }

  private initRouteWatch(): void {
    this.router.events.pipe(filter(event => event instanceof NavigationEnd)).subscribe(() => {
      const routes: string[] = [];
      const parseRoute = (r: ActivatedRouteSnapshot) => {
        if (r.url.length) {
          routes.push(r.url[0].path);
        }
        if (r.children) {
          r.children.forEach((rc) => parseRoute(rc));
        }
      };
      parseRoute(this.route.snapshot);
      this.appService.activeRoute = routes;
    });

    this.router.events.pipe(filter(event => event instanceof NavigationStart)).subscribe(() => {
      this.appService.cancelHttpRequests.next();
    });
  }

}
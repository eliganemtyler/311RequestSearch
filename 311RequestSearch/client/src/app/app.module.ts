import { BrowserModule } from '@angular/platform-browser';
import { APP_INITIALIZER, CUSTOM_ELEMENTS_SCHEMA, NgModule } from '@angular/core';
import '@angular/localize/init';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { Router } from '@angular/router';
import { AppService } from './app.service';
import { HttpClientModule } from '@angular/common/http';
import { MenuComponent } from './shared/components/menu/menu.component';
import { HeaderComponent } from './shared/components/header/header.component';

const initializeAppFactory = (
  router: Router,
  appService: AppService
) => () => { }; //todo
// new Promise<void>((resolve, reject) => {
//   appService.getCurrentUser().subscribe({
//     next: () => {
//       appService.hasAccess = true;
//       resolve();
//     },
//     error: () => {
//       router.navigate(['unauthorized']);
//       resolve();
//     }
//   });
// }
// );

@NgModule({
  declarations: [
    AppComponent,
    HeaderComponent,
    MenuComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    AppRoutingModule,
    BrowserAnimationsModule],
  providers: [
    {
      provide: APP_INITIALIZER,
      useFactory: initializeAppFactory,
      multi: true,
      deps: [Router, AppService]
    }
  ],
  bootstrap: [AppComponent],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class AppModule { }
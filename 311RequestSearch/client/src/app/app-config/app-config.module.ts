import { NgModule, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AppConfigRoutingModule } from './app-config-routing.module';
import { AppConfigComponent } from './app-config/app-config.component';

@NgModule({
  declarations: [AppConfigComponent],
  imports: [
    CommonModule,
    AppConfigRoutingModule
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class AppConfigModule { }

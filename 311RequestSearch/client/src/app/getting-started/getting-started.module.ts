import { NgModule, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GettingStartedRoutingModule } from './getting-started-routing.module';
import { GettingStartedComponent } from './getting-started/getting-started.component';

@NgModule({
  declarations: [GettingStartedComponent],
  imports: [
    CommonModule,
    GettingStartedRoutingModule
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class GettingStartedModule { }

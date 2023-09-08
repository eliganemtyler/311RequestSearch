import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AppConfigComponent } from './app-config/app-config.component';

const routes: Routes = [
  { path: '', component: AppConfigComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AppConfigRoutingModule { }

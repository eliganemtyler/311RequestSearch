import { Component, ElementRef, OnInit } from '@angular/core';
import { AppService } from 'src/app/app.service';
import { isDefined } from '@tylertech/forge-core';
import { Router } from '@angular/router';

@Component({
  selector: '[app-menu]',
  templateUrl: './menu.component.html',
  styleUrls: ['./menu.component.scss']
})
export class MenuComponent implements OnInit {

  constructor(
    private router: Router,
    private elementRef: ElementRef,
    public appService: AppService) { }

  ngOnInit(): void {
  }

  public menuItemSelected(option: string): void {
    if (isDefined(option)) {
      this.router.navigate([option]);
    }
  }

  public expand() {
    this.appService.menu.type = this.appService.menu.type === 'dismissible' ? 'mini' : 'dismissible';
    if (this.appService.menu.type === 'mini') {
      (this.elementRef.nativeElement as HTMLElement).querySelectorAll('forge-expansion-panel').forEach(element => element.open = false);
    }
  }

}
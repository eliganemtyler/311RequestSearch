import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { IAppLauncherOption } from '@tylertech/forge-internal';
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AppService {

  private _intentsUrl: string;
  public activeRoute: string[] = [];
  public cancelHttpRequests = new Subject<void>();


  public menu = { //todo options here
    // 'permanent' | 'dismissible' | 'modal' | 'mini' | 'min-hover' = 'dismissible'
    type: 'dismissible' as 'dismissible' | 'mini',
    options: [
      { label: 'option1', value: 'pets', icon: 'pets' },
      {
        label: 'option2', icon: 'child_friendly',
        options: [
          { label: 'Parent', value: 'test/parent', icon: 'home', leadingIconType: 'component' },
          { label: 'Child', value: 'test/child', icon: 'person', leadingIconType: 'component' }
        ]
      },
    ]
  };

  constructor(private http: HttpClient) {
    this._intentsUrl = 'api/applauncher';
  }

  getIntents() {
    return this.http.get<IAppLauncherOption[]>(this._intentsUrl);
  }
}
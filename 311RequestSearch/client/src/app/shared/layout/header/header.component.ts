import { Component, Inject } from '@angular/core';
import { CookieService } from 'ngx-cookie-service';
import { map } from 'rxjs/operators';
import {
  IMenuOption,
  IMenuSelectEventData,
  IconRegistry
} from '@tylertech/forge';
import { AppUser } from 'src/bootstrap-identity';
import { IAppLauncherOption, AppLauncherOptionsCallback } from '@tylertech/forge-internal';
import { tylIconTylerTalkingTLogo } from '@tylertech/tyler-icons/custom';
import { Config } from 'src/app/core/models/config';
import { APP_CONFIG, IDENTITY } from 'src/app/app.tokens';
import { localeLanguageMap, SupportedLocales } from 'src/app/app.locales';
import { AppLauncherService } from 'src/app/core/services/app-launcher.service';
import { IProfile } from 'src/identity-helper';

const userLocaleCookie = '.User.Locale';

/**
 * Header component
 */
@Component({
  selector: 'tyl-header',
  styleUrls: ['./header.component.scss'],
  templateUrl: './header.component.html'
})
export class HeaderComponent {
  public title = '311RequestSearch';
  public isSignedIn: boolean;
  public profile: IProfile | null;
  public languageOptions: IMenuOption[];

  /**
   * Create an instance of {@link HeaderComponent}
   */
  public constructor(
    @Inject(APP_CONFIG) private readonly _config: Config,
    @Inject(IDENTITY) private readonly _user: AppUser,
    private _cookieService: CookieService,
    private _appLauncherService: AppLauncherService
  ) {
    IconRegistry.define(tylIconTylerTalkingTLogo);
    this.title = this._config.title;
    this.isSignedIn = this._user.isSignedIn;
    this.profile = this._user.profile;

    // setup language switcher menu options
    const defaultLocale = this._config.defaultLanguage;
    const currentLocale = this._cookieService.get(userLocaleCookie) || defaultLocale;
    this.languageOptions = (localeLanguageMap[currentLocale as SupportedLocales] || localeLanguageMap[defaultLocale])
      .map(({ locale: value, label }) => ({ value, label, selected: value === currentLocale }));
  }

  public omnibarLauncherOptions: AppLauncherOptionsCallback = () => this._appLauncherService.getIntents().pipe(
    map((result: IAppLauncherOption[]) => ({ options: result }))
  )
    .toPromise();

  public languageSelected = (data: IMenuSelectEventData): void => {
    const today = new Date();
    const nextYear = new Date(today.setFullYear(today.getFullYear() + 1));
    this._cookieService.set(
      userLocaleCookie,
      data.value,
      nextYear,
      '/',
      window.location.hostname.split('.').slice(-2).join('.'),
      false,
      'Lax'
    );
    window.location.reload();
  };

  /**
   * Redirect to the signin page.
   */
  public signIn(): void {
    window.location.replace('signin');
  }

  /**
   * Redirect to the signout page.
   */
  public signOut(): void {
    window.location.replace('signout');
  }

  /**
   * Redirect to the tcp profile page.
   */
  public viewProfile(): void {
    window.location.href = this._config.viewProfileUri;
  }
}

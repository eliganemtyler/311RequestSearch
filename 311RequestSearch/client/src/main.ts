/* eslint spaced-comment: "off" */

import {
  enableProdMode,
  ValueProvider,
  FactoryProvider,
} from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
import { datadogRum } from '@datadog/browser-rum';
import { OneTrustBootstrapper } from '@tylertech/tyler-cookie-consent';
import { AppModule } from './app/app.module';
import { environment } from './environments/environment';
import { APP_CONFIG, IDENTITY } from './app/app.tokens';

import { bootstrapIdentity } from './bootstrap-identity';
import { BootstrapParameters, bootstrapTylerWebApp } from './bootstrap-tyler-webapp';
import { deleteCookie } from './bootstrap-helpers';

import { defineComponents } from './define-components';
import { defineIcons } from './define-icons';

declare global {
  interface Window {
    configPromise: Promise<any>;
    OnetrustActiveGroups: string;
  }
}

enum OneTrustCookieCategory { //todo need?
  Analytics = 'ANA',
  Functional = 'FUN',
  Marketing = 'MAR',
  Required = 'REQ',
  SocialMedia = 'SOC'
}

if (environment.production) {
  enableProdMode();
}

async function prebootTylerWebApp(configPromise: Promise<any>) {
  const preloadWork = await import('./preboot-index');
  const result = await preloadWork.prebootTylerWebApp(configPromise);
  return result;
}

// function to bridge from generic bootstrapping process into angular
// bootstrap process
async function bootstrapTylerAngular(bootstrapData: BootstrapParameters) {
  const { config, identityFactory } = bootstrapData;

  const configProvider: ValueProvider = {
    provide: APP_CONFIG,
    useValue: config
  };

  const identityProvider: FactoryProvider = {
    provide: IDENTITY,
    useFactory: identityFactory,
    deps: []
  };

  const tokens = [
    configProvider,
    identityProvider,
  ];

  const initDataDog = () => {
    const { version = '0.0.0' } = config;
    const { config: rumConfig } = config?.datadogRum;
    const enableDatadog = rumConfig && rumConfig.applicationId && rumConfig.clientToken && rumConfig.site;
    if (enableDatadog) {
      try {
        rumConfig.version = version;
        datadogRum.init(rumConfig);
      } catch (err) {
        console.error('Failed to initialize RUM.', err);
      }
    }
  };

  defineComponents();
  defineIcons();

  const bootstrap = () => platformBrowserDynamic(tokens).bootstrapModule(AppModule);
  let dataDogIsInitialized = false;
  await new OneTrustBootstrapper().bootstrap({
    ...config.oneTrustConfig, onPolicyChange: () => {
      const allowDataDog = window.OnetrustActiveGroups?.indexOf(OneTrustCookieCategory.Analytics) > 0;

      if (allowDataDog) {
        if (!dataDogIsInitialized) {
          initDataDog();
          dataDogIsInitialized = true;
        }
      } else {
        if (dataDogIsInitialized) {
          deleteCookie('_dd_s');
          window.location.href = window.location.href;
        }
      }
    }
  });


  await bootstrap().catch((err: any) => console.error(err));
}

const bootstrapConfigFn = () => fetch('api/AppConfig').then(r => r.json());

// run our bootstrapped application
bootstrapTylerWebApp(
  prebootTylerWebApp,
  bootstrapConfigFn,
  bootstrapIdentity,
  bootstrapTylerAngular
);

// Generated on 05/12/2023 with Armored Armadillo 1.0.0-dev
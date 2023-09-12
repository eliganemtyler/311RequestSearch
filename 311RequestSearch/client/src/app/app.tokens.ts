import { InjectionToken } from '@angular/core';
import { Config } from './core/models/config';
import { AppUser } from 'src/bootstrap-identity';

// application config is injected at bootstrap time, check main.ts
export const APP_CONFIG = new InjectionToken<Config>('Application Config');

// user data is injected at bootstrap time, check main.ts
export const IDENTITY = new InjectionToken<AppUser>('Identity');
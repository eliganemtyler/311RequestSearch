import { Config } from './app/core/models/config';

export type BootstrapConfigFn = () => Promise<any>;
export interface IIdentityUser<ProfileType> {
  readonly profile: ProfileType | null;

  readonly isSignedIn: boolean;
}

export type PrebootTylerWebAppFn = (configPromise: Promise<any>) => Promise<{ isAppAvailable: boolean; redirectToSignIn: boolean }>;
export type IdentityUser<ProfileType = any> = IIdentityUser<ProfileType> | null | undefined;
export type IdentityFactoryFn<ProfileType = any, RawType = any> = () => IdentityUser<ProfileType>;

export type BootstrapIdentityFn<TConfig = any> = (config: TConfig) => Promise<IdentityFactoryFn>;

export interface BootstrapParameters {
  config: Config;
  identityFactory: IdentityFactoryFn;
}

export type BootstrapWebAppFn = (params: BootstrapParameters) => Promise<any>;

export async function bootstrapTylerWebApp(
  prebootTylerWebAppFn: PrebootTylerWebAppFn,
  bootstrapConfigFn: BootstrapConfigFn,
  bootstrapIdentityFn: BootstrapIdentityFn,
  bootstrapWebAppFn: BootstrapWebAppFn
) {
  const preboot = await prebootTylerWebAppFn(bootstrapConfigFn());

  if (preboot.redirectToSignIn) {
    // Will redirect to signin, ignore loading angular to avoid flashing
    return;
  }

  const config = await bootstrapConfigFn();
  const identityFactory = await bootstrapIdentityFn(config);

  const params: BootstrapParameters = {
    config,
    identityFactory
  };

  bootstrapWebAppFn(params);
}

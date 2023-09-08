import { IdentityHelper, IUser, IProfile } from './identity-helper';
import { IIdentityUser, IdentityFactoryFn } from './bootstrap-tyler-webapp';

export class AppUser implements IIdentityUser<IProfile> {

  public constructor(private readonly helper: IdentityHelper) {}

  private get _user() {
    return this.helper.currentUser;
  }

  get profile() {
    if (this._user) {
      return this._user.profile;
    }
    return null;
  }
  get isSignedIn() {
    return !!this._user;
  }
}

export async function bootstrapIdentity(config: any): Promise<IdentityFactoryFn<IProfile>> {
  const identityHelper = new IdentityHelper();
  await identityHelper.checkAutoLogin();
  return () => new AppUser(identityHelper);
}

export class IdentityHelper {
  currentUser: IUser | undefined | null;

  async signin(referrer: any) {
    // referrer here for backward compatibility, maybe remove
    window.location.replace('signin'); // "http://dev.localdev.tcpci.com:5000/app/new-application/signin");
  }

  async checkAutoLogin() {
    let userInfo: any;
    try {
      userInfo = await fetch('userinfo')
        .then(r => r.json());

      if (!userInfo) {
        // TODO: signin silent in SPA?
      }
      else {
        const user: IUser = {
          profile: {
            username: userInfo.username,
            firstName: userInfo.firstName,
            lastName: userInfo.lastName,
            fullName: `${userInfo.firstName} ${userInfo.lastName}`,
            email: userInfo.email
          }
        };
        this.currentUser = user;
      }


    } catch (error) {
    }
  }
}

export interface IUser {
  profile: IProfile;
}

export interface IProfile {
  username: string;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
}

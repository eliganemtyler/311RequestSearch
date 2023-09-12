import {
  defineComponents as defineForgeComponents
} from '@tylertech/forge';

import {
  defineAppLauncherButtonComponent,
  defineAppLauncherComponent
} from '@tylertech/forge-internal';

export const defineComponents = () => {
  defineForgeComponents();

  defineAppLauncherButtonComponent();
  defineAppLauncherComponent();
}
/* eslint spaced-comment: "off" */

import { RumUserConfiguration } from '@datadog/browser-rum';

export interface OmnibarSettings {
  baseUriService: string;
}

export interface DatadogRum {
  config: RumUserConfiguration;
}
export interface IOneTrustConfig {
  src: string;
  dataDomainScript: string;
  maxWaitTime?: number;
}

export interface Config {
  title: string;
  defaultLanguage: string;
  version: string;
  notFoundPageUri: string;
  viewProfileUri: string;
  brandingServiceUri: string;
  tenantId: string;
  datadogRum: DatadogRum;
  oneTrustConfig: IOneTrustConfig;
}

export interface IFooterLink {
  name: string,
  url: string
}

export interface IPortalConfig {
  footerLinks: IFooterLink[]
}
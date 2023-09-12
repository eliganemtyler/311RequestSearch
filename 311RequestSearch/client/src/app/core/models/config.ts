/* eslint spaced-comment: "off" */

import { RumUserConfiguration } from '@datadog/browser-rum';

export interface OmnibarSettings {
  baseUriService: string;
}

export interface DatadogRum {
  config: RumUserConfiguration;
}
export interface IOneTrustConfig { //delete
  src: string;
  dataDomainScript: string;
  maxWaitTime?: number;
}

export interface Config { //probably only using this
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

export interface IFooterLink { //probably delete
  name: string,
  url: string
}

export interface IPortalConfig {
  footerLinks: IFooterLink[]
}
import { IconRegistry } from '@tylertech/forge';

import {
  tylIconAdd
} from "@tylertech/tyler-icons/standard";

import {
  tylIconTylerTalkingTLogo
} from '@tylertech/tyler-icons/custom';

import {
  tylIconAccountSupervisor,
  tylIconFileDocumentEditOutline,
  tylIconSchool
} from "@tylertech/tyler-icons/extended";

const standardIcons = [
  tylIconAdd
];

const extendedIcons = [
  tylIconFileDocumentEditOutline,
  tylIconAccountSupervisor,
  tylIconSchool
];

const customIcons = [
  tylIconTylerTalkingTLogo
];

export const defineIcons = () => {
  IconRegistry.define([...standardIcons, ...extendedIcons, ...customIcons]);
};
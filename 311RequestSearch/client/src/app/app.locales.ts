export type SupportedLocales = 'en' | 'es-MX'; // count should match LocaleLabels tuple length
export type DefaultLocale = string;
export type LocaleLabels = [ILocaleLabel, ILocaleLabel]; // tuple length should match SupportedLocales count
export interface ILocaleLabel {
  locale: SupportedLocales;
  label: string;
}
export type LanguageList = Record<SupportedLocales | DefaultLocale, LocaleLabels>;

// need to make sure the labels in the LanguageList are pre-translated
// since the i18n extraction does not match cross localizations
export const localeLanguageMap: LanguageList = {
  en: [
    { locale: 'en', label: 'English' },
    { locale: 'es-MX', label: 'Español - (Spanish)' },
  ],
  'es-MX': [
    { locale: 'es-MX', label: 'Español' },
    { locale: 'en', label: 'English - (Inglés)' },
  ]
};

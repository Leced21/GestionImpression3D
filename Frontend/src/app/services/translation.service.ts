import { Injectable, signal } from '@angular/core';
import { SupportedLang, TRANSLATIONS } from '../i18n/translations';

const STORAGE_KEY = 'appLanguage';

@Injectable({ providedIn: 'root' })
export class TranslationService {
  private readonly lang = signal<SupportedLang>(this.readInitialLang());
  readonly currentLang = this.lang.asReadonly();

  private readInitialLang(): SupportedLang {
    return this.normalize(localStorage.getItem(STORAGE_KEY));
  }

  private normalize(lang: string | null | undefined): SupportedLang {
    return lang === 'en' ? 'en' : 'fr';
  }

  use(lang: string): void {
    const normalized = this.normalize(lang);
    this.lang.set(normalized);
    localStorage.setItem(STORAGE_KEY, normalized);
  }

  translate(key: string): string {
    return TRANSLATIONS[this.lang()][key] ?? TRANSLATIONS.fr[key] ?? key;
  }
}

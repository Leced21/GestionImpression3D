import { Pipe, PipeTransform, inject } from '@angular/core';
import { TranslationService } from '../services/translation.service';

// Impure: re-evaluated on every change detection cycle so text updates the
// moment the user switches language, without needing an async pipe.
@Pipe({ name: 'translate', standalone: true, pure: false })
export class TranslatePipe implements PipeTransform {
  private readonly translation = inject(TranslationService);

  transform(key: string): string {
    return this.translation.translate(key);
  }
}

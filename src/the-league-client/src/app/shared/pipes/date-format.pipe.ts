import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'dateFormat',
  standalone: true,
})
export class DateFormatPipe implements PipeTransform {
  transform(value: string | Date | null | undefined): string {
    if (!value) return '';

    const date = value instanceof Date ? value : new Date(value);
    if (isNaN(date.getTime())) return '';

    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffHours = diffMs / (1000 * 60 * 60);

    if (diffHours >= 0 && diffHours < 48) {
      if (diffHours < 1) {
        const mins = Math.floor(diffMs / (1000 * 60));
        return mins <= 1 ? 'just now' : `${mins} minutes ago`;
      }
      const hours = Math.floor(diffHours);
      return hours === 1 ? '1 hour ago' : `${hours} hours ago`;
    }

    const day = date.getDate().toString().padStart(2, '0');
    const month = (date.getMonth() + 1).toString().padStart(2, '0');
    const year = date.getFullYear();
    return `${day}/${month}/${year}`;
  }
}

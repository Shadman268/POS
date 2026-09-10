import { Pipe, PipeTransform } from '@angular/core';

export function formatAmount(value: number | string | null | undefined): string {
  const numeric = Number(value);
  if (!isFinite(numeric)) {
    return '0';
  }

  const rounded = Math.round((numeric + Number.EPSILON) * 10) / 10;
  if (Number.isInteger(rounded)) {
    return String(rounded);
  }

  return rounded.toFixed(1);
}

@Pipe({
  name: 'amount'
})
export class AmountPipe implements PipeTransform {
  transform(value: number | string | null | undefined): string {
    return formatAmount(value);
  }
}

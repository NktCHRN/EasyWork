import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'dayOfWeek'
})
export class DayOfWeekPipe implements PipeTransform {

  transform(value: number, format: 'long' | 'short' = 'long'): string {
    if (value < 0 || value > 6)
      return '';
    const days = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
    const day = days[value];
    return format === 'short' ? day.substring(0, 1) : day;
  }

}

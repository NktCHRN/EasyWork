import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class TimeService {

  constructor() { }

  public daysToMilliseconds(days: number)
   {
    return days * 24 * 60 * 60 * 1000;
   }
}

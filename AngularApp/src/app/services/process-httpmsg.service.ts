import { HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { throwError } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ProcessHTTPMsgService {

  constructor() { }

  public handleError(error: HttpErrorResponse | any) {
    let newError: Error;

    if (error.error instanceof ErrorEvent || error.error?.message) {
      newError = Error(error.error.message);
    }
    else if ( error.error?.errorMessage && !error.error?.errorDetails) {
      newError = Error(error.error?.errorMessage);
    } 
    else {
      newError = error;
    }

    return throwError(() => newError);
  }
}

import { HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { throwError } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ProcessHTTPMsgService {

  constructor() { }

  public handleError(error: HttpErrorResponse | any) {
    let errMsg: string;

    if (error.error instanceof ErrorEvent || error.error?.message) {
      errMsg = error.error.message;
    }
    else if ( error.error?.errorMessage) {
      errMsg = error.error?.errorMessage
    } 
    else {
      errMsg = `${error.status} - ${error.statusText || ''}\n${JSON.stringify(error.error)}`;
    }

    return throwError(() => new Error(errMsg));
  }
}

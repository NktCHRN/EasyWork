import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable } from 'rxjs';
import { AuthenticatedResponse } from '../shared/authenticatedresponse';
import { CustomEncoder } from '../shared/customencoder';
import { LoginModel } from '../shared/loginmodel';
import { RegisterUser } from '../shared/registeruser';
import { ResendEmailConfirmation } from '../shared/resendemailconfirmation';
import { BaseService } from './base.service';
import { ProcessHTTPMsgService } from './process-httpmsg.service';

@Injectable({
  providedIn: 'root'
})
export class AccountService extends BaseService {

  constructor(private http: HttpClient,
    private processHTTPMsgService: ProcessHTTPMsgService) {
    super();
  }

  override serviceBaseURL: string = this.baseURL + 'Account/';

  public register(user: RegisterUser): Observable<RegisterUser>
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json'
      })
    };
    return this.http.post<RegisterUser>(this.serviceBaseURL + 'register', user, httpOptions)
      .pipe(catchError(this.processHTTPMsgService.handleError));
  }

  public login(user: LoginModel): Observable<AuthenticatedResponse> {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json'
      })
    };
    return this.http.post<AuthenticatedResponse>(this.serviceBaseURL + 'login', user, httpOptions)
      .pipe(catchError(this.processHTTPMsgService.handleError));
  }

  public confirmEmail = (token: string, email: string) => {
    let params = new HttpParams({ encoder: new CustomEncoder() })
    params = params.append('token', token);
    params = params.append('email', email);
    return this.http.get(this.serviceBaseURL + "EmailConfirmation", { params: params }).pipe(catchError(this.processHTTPMsgService.handleError));
  }

  public resendEmail(model: ResendEmailConfirmation) {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json'
      })
    };
    return this.http.post<ResendEmailConfirmation>(this.serviceBaseURL + 'EmailConfirmationMessageResend', model, httpOptions)
      .pipe(catchError(this.processHTTPMsgService.handleError));
  }

}

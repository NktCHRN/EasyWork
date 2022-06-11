import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable, Subject } from 'rxjs';
import { AuthenticatedResponse } from '../shared/authenticatedresponse';
import { CustomEncoder } from '../shared/customencoder';
import { LoginModel } from '../shared/loginmodel';
import { RegisterUser } from '../shared/registeruser';
import { ResendEmailConfirmation } from '../shared/resendemailconfirmation';
import { BaseService } from './base.service';
import { ProcessHTTPMsgService } from './process-httpmsg.service';
import { SocialAuthService } from "angularx-social-login";
import { GoogleLoginProvider } from "angularx-social-login";
import { JwtHelperService } from '@auth0/angular-jwt';
import { ExternalAuthModel } from '../shared/externalauthmodel';

@Injectable({
  providedIn: 'root'
})
export class AccountService extends BaseService {

  private _authChangeSub = new Subject<boolean>()
  public authChanged = this._authChangeSub.asObservable();

  constructor(private http: HttpClient,
    private processHTTPMsgService: ProcessHTTPMsgService,
    private jwtHelper: JwtHelperService, private _externalAuthService: SocialAuthService) {
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

  public signInWithGoogle = ()=> {
    return this._externalAuthService.signIn(GoogleLoginProvider.PROVIDER_ID);
  }

  public signOutExternal = () => {
    this._externalAuthService.signOut();
  }

  public externalLogin = (body: ExternalAuthModel) => {
    return this.http.post<AuthenticatedResponse>(this.serviceBaseURL + "ExternalLogin", body);
  }

  public isUserAuthenticated = async (): Promise<boolean> => {
    const token = localStorage.getItem("jwt");    
    if (token && !this.jwtHelper.isTokenExpired(token)){
      return true;
    }
    return false;
  }

  public logout = () => {
    localStorage.removeItem("jwt");
    localStorage.removeItem("refreshToken");
  }

  public sendAuthStateChangeNotification = (isAuthenticated: boolean) => {
    this._authChangeSub.next(isAuthenticated);
  }

}

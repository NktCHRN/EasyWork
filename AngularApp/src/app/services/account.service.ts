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
import { UserModel } from '../shared/user.model';
import { UpdateUser } from '../shared/update-user';
import { ForgotPasswordModel } from '../shared/forgot-password.model';
import { ResetPasswordModel } from '../shared/reset-password.model';

@Injectable({
  providedIn: 'root'
})
export class AccountService extends BaseService {

  private _authChangeSub = new Subject<boolean>()
  public authChanged = this._authChangeSub.asObservable();

  constructor(private _http: HttpClient,
    private _processHTTPMsgService: ProcessHTTPMsgService,
    private _jwtHelper: JwtHelperService, private _externalAuthService: SocialAuthService) {
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
    return this._http.post<RegisterUser>(this.serviceBaseURL + 'register', user, httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }

  public login(user: LoginModel): Observable<AuthenticatedResponse> {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json'
      })
    };
    return this._http.post<AuthenticatedResponse>(this.serviceBaseURL + 'login', user, httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }

  public confirmEmail = (token: string, email: string) => {
    let params = new HttpParams({ encoder: new CustomEncoder() })
    params = params.append('token', token);
    params = params.append('email', email);
    return this._http.get(this.serviceBaseURL + "EmailConfirmation", { params: params }).pipe(catchError(this._processHTTPMsgService.handleError));
  }

  public resendEmail(model: ResendEmailConfirmation) {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json'
      })
    };
    return this._http.post(this.serviceBaseURL + 'EmailConfirmationMessageResend', model, httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }

  public forgotPassword(model: ForgotPasswordModel) : Observable<Object>
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json'
      })
    };
    return this._http.post(this.serviceBaseURL + 'forgotpassword', model, httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }

  public resetPassword(model: ResetPasswordModel) : Observable<Object>
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json'
      })
    };
    return this._http.post(this.serviceBaseURL + 'resetpassword', model, httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }

  public signInWithGoogle = ()=> {
    return this._externalAuthService.signIn(GoogleLoginProvider.PROVIDER_ID);
  }

  public signOutExternal = () => {
    this._externalAuthService.signOut();
  }

  public externalLogin = (body: ExternalAuthModel) => {
    return this._http.post<AuthenticatedResponse>(this.serviceBaseURL + "ExternalLogin", body);
  }

  public isUserAuthenticated = async (): Promise<boolean> => {
    const token = localStorage.getItem("jwt");    
    if (token && !this._jwtHelper.isTokenExpired(token)){
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

  public get(token: string) : Observable<UserModel>
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': 'Bearer ' + token
      })
    };
    return this._http.get<UserModel>(this.serviceBaseURL, httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }

  public update(token: string, user: UpdateUser)
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': 'Bearer ' + token
      })
    };
    return this._http.put(this.serviceBaseURL, user, httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }

  public deleteAvatar(token: string) {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': 'Bearer ' + token
      })
    };
    return this._http.delete(this.serviceBaseURL + "Avatar", httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }

  public updateAvatar(token: string, file: FormData)
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Authorization': 'Bearer ' + token
      })
    };
    return this._http.put(this.serviceBaseURL + "Avatar", file, httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }
}

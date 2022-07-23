import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable, Subject } from 'rxjs';
import { AuthenticatedResponseModel } from '../shared/token/authenticated-response.model';
import { CustomEncoder } from '../shared/other/customencoder';
import { LoginModel } from '../shared/user/login.model';
import { RegisterUserModel } from '../shared/user/register-user.model';
import { ResendEmailConfirmationModel } from '../shared/user/resend-email-confirmation.model';
import { BaseService } from './base.service';
import { ProcessHTTPMsgService } from './process-httpmsg.service';
import { SocialAuthService } from "angularx-social-login";
import { GoogleLoginProvider } from "angularx-social-login";
import { JwtHelperService } from '@auth0/angular-jwt';
import { ExternalAuthModel } from '../shared/token/external-auth.model';
import { UserCabinetModel } from '../shared/user/cabinet/user-cabinet.model';
import { UpdateUserModel } from '../shared/user/update-user.model';
import { ForgotPasswordModel } from '../shared/user/forgot-password.model';
import { ResetPasswordModel } from '../shared/user/reset-password.model';
import { TokenService } from './token.service';

@Injectable({
  providedIn: 'root'
})
export class AccountService extends BaseService {

  private _authChangeSub = new Subject<boolean>()
  public authChanged = this._authChangeSub.asObservable();

  constructor(private _http: HttpClient,
    private _processHTTPMsgService: ProcessHTTPMsgService,
    private _jwtHelper: JwtHelperService, private _externalAuthService: SocialAuthService,
    private _tokenService: TokenService) {
    super();
  }

  override serviceBaseURL: string = this.baseURL + 'Account/';

  public register(user: RegisterUserModel): Observable<RegisterUserModel>
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json'
      })
    };
    return this._http.post<RegisterUserModel>(this.serviceBaseURL + 'register', user, httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }

  public login(user: LoginModel): Observable<AuthenticatedResponseModel> {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json'
      })
    };
    return this._http.post<AuthenticatedResponseModel>(this.serviceBaseURL + 'login', user, httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }

  public confirmEmail = (token: string, email: string) => {
    let params = new HttpParams({ encoder: new CustomEncoder() })
    params = params.append('token', token);
    params = params.append('email', email);
    return this._http.get(this.serviceBaseURL + "EmailConfirmation", { params: params }).pipe(catchError(this._processHTTPMsgService.handleError));
  }

  public resendEmail(model: ResendEmailConfirmationModel) {
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
    return this._http.post<AuthenticatedResponseModel>(this.serviceBaseURL + "ExternalLogin", body);
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

  public get() : Observable<UserCabinetModel>
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': this._tokenService.getAuthHeaderValue()
      })
    };
    return this._http.get<UserCabinetModel>(this.serviceBaseURL, httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }

  public update(user: UpdateUserModel)
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': this._tokenService.getAuthHeaderValue()
      })
    };
    return this._http.put(this.serviceBaseURL, user, httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }

  public deleteAvatar() {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': this._tokenService.getAuthHeaderValue()
      })
    };
    return this._http.delete(this.serviceBaseURL + "Avatar", httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }

  public updateAvatar(file: FormData)
  {
    const httpOptions = {
      headers: new HttpHeaders({
        'Authorization': this._tokenService.getAuthHeaderValue()
      })
    };
    return this._http.put(this.serviceBaseURL + "Avatar", file, httpOptions)
      .pipe(catchError(this._processHTTPMsgService.handleError));
  }
}

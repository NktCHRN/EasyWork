import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { JwtHelperService } from '@auth0/angular-jwt';
import { Observable } from 'rxjs';
import { RevokeTokenModel } from '../shared/token/revoke-token.model';
import { TokenResponseModel } from '../shared/token/token-response.model';
import { BaseService } from './base.service';

@Injectable({
  providedIn: 'root'
})
export class TokenService extends BaseService {

  constructor(private _http: HttpClient, private _jwtService: JwtHelperService) {
    super();
  }

  override serviceBaseURL: string = this.baseURL + 'Token/';
  
  public refreshToken(credentials: TokenResponseModel): Observable<TokenResponseModel> {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json'
      })
    };
    return this._http.post<TokenResponseModel>(this.serviceBaseURL + 'refresh', credentials, httpOptions)
  }

  public revokeToken(token: string, tokenModel: RevokeTokenModel) : Observable<Object> {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': 'Bearer ' + token
      })
    };
    return this._http.post(this.serviceBaseURL + 'revoke', tokenModel, httpOptions)
  }

  public getJwtToken(): string | null | undefined
  {
    return this._jwtService.tokenGetter();
  }

  public getAuthHeaderValue(): string {
    return 'Bearer ' + this.getJwtToken();
  }

  public getRefreshToken(): string | null | undefined
  {
    return localStorage.getItem("refreshToken");
  }

  public getTokens(): TokenResponseModel
  {
    return {
      accessToken: this.getJwtToken()!,
      refreshToken: this.getRefreshToken()!
    };
  }

  public setTokens(tokens: TokenResponseModel): void
  {
    localStorage.setItem("jwt", tokens.accessToken);
    localStorage.setItem("refreshToken", tokens.refreshToken); 
  }

  public getMyId(): number | null | undefined {
    const token = this.getJwtToken();
    let id = undefined;
    if (token)
    {
      const decoded = this._jwtService.decodeToken();
      id = decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"];
    }
    return id;
  }
}

import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { RevokeTokenModel } from '../shared/revoke-token.model';
import { TokenResponse } from '../shared/tokenresponse';
import { BaseService } from './base.service';

@Injectable({
  providedIn: 'root'
})
export class TokenService extends BaseService {

  constructor(private http: HttpClient) {
    super();
  }

  override serviceBaseURL: string = this.baseURL + 'Token/';
  
  public refreshToken(credentials: TokenResponse): Observable<TokenResponse> {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json'
      })
    };
    return this.http.post<TokenResponse>(this.serviceBaseURL + 'refresh', credentials, httpOptions)
  }

  public revokeToken(token: string, tokenModel: RevokeTokenModel) : Observable<Object> {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type':  'application/json',
        'Authorization': 'Bearer ' + token
      })
    };
    return this.http.post(this.serviceBaseURL + 'revoke', tokenModel, httpOptions)
  }

  public getJwtToken(): string | null | undefined
  {
    return localStorage.getItem('jwt');
  }

  public getRefreshToken(): string | null | undefined
  {
    return localStorage.getItem("refreshToken");
  }

  public getTokens(): TokenResponse
  {
    return {
      accessToken: this.getJwtToken()!,
      refreshToken: this.getRefreshToken()!
    };
  }

  public setTokens(tokens: TokenResponse): void
  {
    localStorage.setItem("jwt", tokens.accessToken);
    localStorage.setItem("refreshToken", tokens.refreshToken); 
  }
}

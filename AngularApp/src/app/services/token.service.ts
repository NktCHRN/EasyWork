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
}

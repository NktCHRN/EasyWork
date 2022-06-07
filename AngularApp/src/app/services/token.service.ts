import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AuthenticatedResponse } from '../shared/authenticatedresponse';
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
}

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

  public async tryRefreshingTokens(token: string): Promise<boolean> {
    // Try refreshing tokens using refresh token
    const refreshToken: string = localStorage.getItem("refreshToken")!;
    if (!token || !refreshToken) { 
      return false;
    }
    const credentials: TokenResponse = { accessToken: token, refreshToken: refreshToken };
    let isRefreshSuccess: boolean = false;

    try {
      const refreshRes = await new Promise<TokenResponse>((resolve, reject) => {
        this.refreshToken(credentials).subscribe({
          next: (res: TokenResponse) => resolve(res),
          error: (_) => { reject(); isRefreshSuccess = false;}
        });
      })
      localStorage.setItem("jwt", refreshRes.accessToken);
      localStorage.setItem("refreshToken", refreshRes.refreshToken);
      isRefreshSuccess = true;
    }
    catch (error) {   }

    return isRefreshSuccess;
  }
}

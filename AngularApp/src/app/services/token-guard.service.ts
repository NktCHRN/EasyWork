import { Injectable } from '@angular/core';
import { JwtHelperService } from '@auth0/angular-jwt';
import { TokenResponse } from '../shared/tokenresponse';
import { AccountService } from './account.service';
import { TokenService } from './token.service';

@Injectable({
  providedIn: 'root'
})
export class TokenGuardService {

  constructor(private accountService: AccountService, private tokenService: TokenService, private jwtHelper: JwtHelperService) {  }

  public async tryRefreshingTokens(token: string): Promise<boolean> {
    // Try refreshing tokens using refresh token
    const refreshToken: string = localStorage.getItem("refreshToken")!;
    if (!token || !refreshToken) { 
      this.accountService.sendAuthStateChangeNotification(false);
      return false;
    }
    const credentials: TokenResponse = { accessToken: token, refreshToken: refreshToken };
    let isRefreshSuccess: boolean = false;

    try {
      const refreshRes = await new Promise<TokenResponse>((resolve, reject) => {
        this.tokenService.refreshToken(credentials).subscribe({
          next: (res: TokenResponse) => resolve(res),
          error: (_) => { reject(); isRefreshSuccess = false;}
        });
      })
      localStorage.setItem("jwt", refreshRes.accessToken);
      localStorage.setItem("refreshToken", refreshRes.refreshToken);
      isRefreshSuccess = true;
    }
    catch (error) {   }
    this.accountService.sendAuthStateChangeNotification(isRefreshSuccess);
    return isRefreshSuccess;
  }

  public async refreshToken()
  {
    const token = localStorage.getItem("jwt");    
    if (!token || this.jwtHelper.isTokenExpired(token))
      await this.tryRefreshingTokens(token!); 
  }
}

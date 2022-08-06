import { Injectable } from '@angular/core';
import { JwtHelperService } from '@auth0/angular-jwt';
import { TokenResponseModel } from '../shared/token/token-response.model';
import { AccountService } from './account.service';
import { TokenService } from './token.service';
import { from } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class TokenGuardService {
  constructor(private _accountService: AccountService, private _tokenService: TokenService, private _jwtHelper: JwtHelperService) {
    }

  public async tryRefreshingTokens(token: string): Promise<boolean> {
    // Try refreshing tokens using refresh token
    const refreshToken: string = localStorage.getItem("refreshToken")!;
    if (!token || !refreshToken) { 
      this._accountService.sendAuthStateChangeNotification(false);
      return false;
    }
    let isRefreshSuccess: boolean = false;
    try {
      await new Promise<TokenResponseModel>(async (resolve, reject) => {
        const result = await this._tokenService.refreshToken();
        result.subscribe({
          next: (res: TokenResponseModel) => resolve(res),
          error: (_) => { reject(); isRefreshSuccess = false; this._accountService.logout();}
        });
      }
      )
      isRefreshSuccess = true;
    }
    catch (error) { }
    this._accountService.sendAuthStateChangeNotification(isRefreshSuccess);
    return isRefreshSuccess;
  }

  public async refreshToken()
  {
    const token = localStorage.getItem("jwt");    
    if (!token || this._jwtHelper.isTokenExpired(token))
      await this.tryRefreshingTokens(token!); 
  }

  public async getOrRefreshToken(): Promise<string>
  {
    const token = this._tokenService.getJwtToken()!;
    await this.tryRefreshingTokens(token!); 
    return this._tokenService.getJwtToken()!;
  }
}

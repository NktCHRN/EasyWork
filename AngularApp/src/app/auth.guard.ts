import { AuthenticatedResponse } from './shared/authenticatedresponse';
import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot } from '@angular/router';
import { JwtHelperService } from '@auth0/angular-jwt';
import { TokenResponse } from './shared/tokenresponse';
import { TokenService } from './services/token.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  constructor(private router:Router, private jwtHelper: JwtHelperService, private tokenService: TokenService){}
  
  async canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot) {
    const token = localStorage.getItem("jwt");    
    if (token && !this.jwtHelper.isTokenExpired(token)){
      return true;
    }
    const isRefreshSuccess = await this.tryRefreshingTokens(token!); 
    if (!isRefreshSuccess) { 
      this.router.navigate(["login"]); 
    }
    return isRefreshSuccess;
  }

  private async tryRefreshingTokens(token: string): Promise<boolean> {
    // Try refreshing tokens using refresh token
    const refreshToken: string = localStorage.getItem("refreshToken")!;
    if (!token || !refreshToken) { 
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

    return isRefreshSuccess;
  }
  
}

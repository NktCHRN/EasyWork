import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot } from '@angular/router';
import { JwtHelperService } from '@auth0/angular-jwt';
import { TokenGuardService } from '../services/token-guard.service';
import { TokenService } from '../services/token.service';

@Injectable({
  providedIn: 'root'
})
export class AdminGuard implements CanActivate {
  constructor(private _router:Router, private _jwtHelper: JwtHelperService, private _tokenGuardService: TokenGuardService,
    private _tokenService: TokenService)
  {  }

  async canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot) {
      const token = this._tokenService.getJwtToken();    
      if (!token)
      {
        this.navigateToLoginPage(state);
        return false;
      }
      if (!this._jwtHelper.isTokenExpired(token) && this._tokenService.isAdmin())
        return true;

      await this._tokenGuardService.tryRefreshingTokens(token!); 
      if (!this._tokenService.isAdmin())
      {        
        if (!this._tokenService.getJwtToken())        // token can be changed by the tryRefreshingTokens() method
          this.navigateToLoginPage(state);
        else
          this._router.navigate(["/forbidden"], {skipLocationChange: true}); 
        return false;
      }
      return true;
  }

  private navigateToLoginPage(state: RouterStateSnapshot): void
  {
    this._router.navigate(["login"], {queryParams: {returnUrl: state.url}}); 
  }
}

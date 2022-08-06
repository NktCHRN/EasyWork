import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot } from '@angular/router';
import { JwtHelperService } from '@auth0/angular-jwt';
import { TokenGuardService } from '../services/token-guard.service';

@Injectable({
  providedIn: 'root'
})
export class AnonymousGuard implements CanActivate {
  constructor(private _router:Router, private _jwtHelper: JwtHelperService, private _tokenGuardService: TokenGuardService){}

  private navigateTo: string = "cabinet";

  async canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot) {
    const token = localStorage.getItem("jwt");    
    if (token && !this._jwtHelper.isTokenExpired(token))
    {
      this._router.navigate([this.navigateTo]); 
      return false;
    }
    const isRefreshSuccess = await this._tokenGuardService.tryRefreshingTokens(token!); 
    if (isRefreshSuccess) 
      this._router.navigate([this.navigateTo]); 
    return !isRefreshSuccess;
  }
  
}

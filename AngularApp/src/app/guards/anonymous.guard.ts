import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot, UrlTree } from '@angular/router';
import { JwtHelperService } from '@auth0/angular-jwt';
import { TokenGuardService } from '../services/token-guard.service';

@Injectable({
  providedIn: 'root'
})
export class AnonymousGuard implements CanActivate {
  constructor(private router:Router, private jwtHelper: JwtHelperService, private tokenService: TokenGuardService){}

  private navigateTo: string = "cabinet";

  async canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot) {
    const token = localStorage.getItem("jwt");    
    if (token && !this.jwtHelper.isTokenExpired(token))
      this.router.navigate([this.navigateTo]); 
    const isRefreshSuccess = await this.tokenService.tryRefreshingTokens(token!); 
    if (isRefreshSuccess) 
      this.router.navigate([this.navigateTo]); 
    return true;
  }
  
}

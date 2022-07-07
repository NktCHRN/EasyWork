import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { SocialAuthService } from 'angularx-social-login';
import { AccountService } from '../../services/account.service';
import { TokenService } from '../../services/token.service';
import { UserinfoService } from '../../services/userinfo.service';
import { RevokeTokenModel } from '../../shared/token/revoke-token.model';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent implements OnInit {

  public showRows: boolean = false;
  public isExternalAuth: boolean = false;
  public isUserAuthenticated: boolean | null | undefined;
  
  logOut = () => {
    let model = new RevokeTokenModel();
    model.token = localStorage.getItem("refreshToken")!;
    this._tokenService.revokeToken(localStorage.getItem('jwt')!, model).subscribe();
    this.accountService.logout();
    this.accountService.sendAuthStateChangeNotification(false);
    if(this.isExternalAuth)
      this.accountService.signOutExternal();
    this._router.navigate(['home']);
  }

  constructor(public accountService: AccountService,
    private _socialAuthService: SocialAuthService,
    private _router: Router,
    public userInfoService: UserinfoService,
    private _tokenService: TokenService) { }

  ngOnInit(): void {  
    this.accountService.authChanged
    .subscribe(res => {
      this.onAuthChange(res);
    })
    this._socialAuthService.authState.subscribe((user: any) => {
      this.isExternalAuth = user != null;
    })
    this.accountService.isUserAuthenticated().then(res => this.onAuthChange(res));
  }

   private onAuthChange(res: boolean): void {
    this.isUserAuthenticated = res;
      if (res)
        this.userInfoService.updateLastUser();
   }

  showBtns() : void {
    this.showRows = !this.showRows;
  }
}

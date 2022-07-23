import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { SocialAuthService } from 'angularx-social-login';
import { UserReducedModel } from 'src/app/shared/user/user-reduced.model';
import { AccountService } from '../../services/account.service';
import { TokenService } from '../../services/token.service';
import { UserInfoService } from '../../services/userinfo.service';
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
  public user: UserReducedModel | null | undefined;
  
  logOut = () => {
    let model = new RevokeTokenModel();
    model.token = localStorage.getItem("refreshToken")!;
    const jwt = localStorage.getItem('jwt')!;
    this._tokenService.revokeToken(jwt, model).subscribe();
    this._accountService.logout();
    this._accountService.sendAuthStateChangeNotification(false);
    if(this.isExternalAuth)
      this._accountService.signOutExternal();
    this._router.navigate(['home']);
  }

  constructor(private _accountService: AccountService,
    private _socialAuthService: SocialAuthService,
    private _router: Router,
    private _userInfoService: UserInfoService,
    private _tokenService: TokenService) { }

  ngOnInit(): void {  
    this._accountService.authChanged
    .subscribe(res => {
      this.onAuthChange(res);
    })
    this._socialAuthService.authState.subscribe((user: any) => {
      this.isExternalAuth = user != null;
    })
    this._accountService.isUserAuthenticated().then(res => this.onAuthChange(res));
    this._userInfoService.lastUser.subscribe(user => this.user = user);
  }

   private onAuthChange(res: boolean): void {
    this.isUserAuthenticated = res;
   }

  showBtns() : void {
    this.showRows = !this.showRows;
  }
}

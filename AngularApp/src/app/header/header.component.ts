import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { JwtHelperService } from '@auth0/angular-jwt';
import { SocialAuthService } from 'angularx-social-login';
import { AccountService } from '../services/account.service';
import { UserinfoService } from '../services/userinfo.service';
import { UserReducedModel } from '../shared/user-reduced.model';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent implements OnInit {

  showRows: boolean = false;
  public isExternalAuth: boolean = false;
  public isUserAuthenticated: boolean | null | undefined;
  
  logOut = () => {
    this.accountService.logout();
    this.accountService.sendAuthStateChangeNotification(false);
    if(this.isExternalAuth)
      this.accountService.signOutExternal();
    this.router.navigate(['home']);
  }

  constructor(private jwtHelper: JwtHelperService,
    public accountService: AccountService,
    private socialAuthService: SocialAuthService,
    private router: Router,
    public userInfoService: UserinfoService) { }

  ngOnInit(): void {  
    this.accountService.authChanged
    .subscribe(res => {
      this.onAuthChange(res);
    })
    this.socialAuthService.authState.subscribe((user: any) => {
      this.isExternalAuth = user != null;
    })
    this.accountService.isUserAuthenticated().then(res => this.onAuthChange(res));
  }

   private onAuthChange(res: boolean): void {
    this.isUserAuthenticated = res;
      if (res)
      {
        this.userInfoService.get(localStorage.getItem('jwt')!)
        .subscribe(user => this.userInfoService.setLastUser(user));
      }
   }

  showBtns() : void {
    this.showRows = !this.showRows;
  }
}

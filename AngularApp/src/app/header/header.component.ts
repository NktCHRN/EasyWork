import { Component, ElementRef, OnInit } from '@angular/core';
import { JwtHelperService } from '@auth0/angular-jwt';
import { SocialAuthService } from 'angularx-social-login';
import { AccountService } from '../services/account.service';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent implements OnInit {

  showRows: boolean = false;
  public isExternalAuth: boolean = false;
  public isUserAuthenticated: boolean = false;
  
  logOut = () => {
    this.accountService.logout();
    if(this.isExternalAuth)
      this.accountService.signOutExternal();
  }

  constructor(private jwtHelper: JwtHelperService,
    private accountService: AccountService,
    private socialAuthService: SocialAuthService) { }

  ngOnInit(): void {  
    this.accountService.authChanged
    .subscribe(res => {
      this.isUserAuthenticated = res;
    })
    this.socialAuthService.authState.subscribe((user: any) => {
      this.isExternalAuth = user != null;
    })
  }

  showBtns() : void {
    this.showRows = !this.showRows;
  }
}

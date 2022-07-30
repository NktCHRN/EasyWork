import { Component, Inject } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Router, NavigationEnd, ActivatedRoute } from '@angular/router';
import { filter, map } from 'rxjs';
import { AccountService } from './services/account.service';
import { TokenService } from './services/token.service';
import { UserInfoService } from './services/userinfo.service';
import * as signalR from '@microsoft/signalr';
import { AnonymousGuard } from './guards/anonymous.guard';
import { AuthGuard } from './guards/auth.guard';
import { TokenGuardService } from './services/token-guard.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  connection: signalR.HubConnection | null | undefined;

  constructor(
    private _router: Router,
    private _titleService: Title,
    @Inject('projectName') private _projectName: string,
    private _accountService: AccountService,
    private _userInfoService: UserInfoService,
    @Inject('signalRURL') private _signalRURL: string,
    private _tokenService: TokenService,
    private _tokenGuardService: TokenGuardService,
    private _route: ActivatedRoute
  ) {}

  private _lastAuthStatus: boolean = false;

    private async connect() {
      this.connection = new signalR.HubConnectionBuilder()
        .withUrl(this._signalRURL + "usersHub", {
          skipNegotiation: true,
          transport: signalR.HttpTransportType.WebSockets,
          accessTokenFactory: () => this._tokenGuardService.getOrRefreshToken()
        })
        .withAutomaticReconnect()
        .build();
        this.connection.onreconnected(() => this.login());
        this.connection.start().then(() => this.login()).catch(err => document.write(err));
    }

    private login(): void {
      this.connection!.invoke('Login')
          .catch(error => console.error(error));
    }

   private onAuthChange(res: boolean): void {
      this._lastJwt = this._tokenService.getJwtToken()!;
      if (res != this._lastAuthStatus)
      {
        if (res)
        {
          if (this.connection && this.connection.state == signalR.HubConnectionState.Connected)
            this.connection?.stop().then(() => {
              this.connection = null;
              this.connect();
            });
          else
            this.connect();
          this._userInfoService.updateLastUser();
        }
        else if (this.connection && this.connection.state == signalR.HubConnectionState.Connected)
        {
          this.connection.stop().then(() => this.connection = null);
        }
        this._lastAuthStatus = res;
      }  
   }

   private _lastJwt: string | null | undefined;

   private listenStorageChange() {
    const token = this._tokenService.getJwtToken();
    if (token != this._lastJwt)
    {
      const value = !!token;
      const reload = value != this._lastAuthStatus;
      this._accountService.sendAuthStateChangeNotification(value);
      if (reload)
      {
        const currentUrl = this._router.url;
        if (value && currentUrl.startsWith("/login"))
        {
          let urlParam: string | null | undefined = this._route.snapshot.queryParams['returnUrl'];
          if (urlParam)
            this._router.navigate([urlParam])
          else
            this.reload(currentUrl);
        }
        else
          this.reload(currentUrl);
      }
    }
  }

  private reload(currentUrl: string): void {
    this._router.navigateByUrl("/home", {skipLocationChange: true})
    .then(() => this._router.navigate([currentUrl]));
  }

  ngOnInit() {
    this._router.events
      .pipe(
        filter((event) => event instanceof NavigationEnd),
        map(() => {
          let route: ActivatedRoute = this._router.routerState.root;
          let routeTitle = '';
          while (route!.firstChild) {
            route = route.firstChild;
          }
          if (route.snapshot.data['title']) {
            routeTitle = route!.snapshot.data['title'];
          }
          return routeTitle;
        })
      )
      .subscribe((title: string) => {
        if (title) {
          this._titleService.setTitle(`${this._projectName} - ${title}`);
        }
      });

    this._lastJwt = this._tokenService.getJwtToken();
    window.addEventListener("storage", () => this.listenStorageChange(), false);

    this._accountService.authChanged
    .subscribe(res => {
      this.onAuthChange(res);
    });
    this._accountService.isUserAuthenticated().then(res => this.onAuthChange(res));
  }
}

import { Component, Inject } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Router, NavigationEnd, ActivatedRoute } from '@angular/router';
import { filter, map } from 'rxjs';
import { AccountService } from './services/account.service';
import { TokenService } from './services/token.service';
import { UserInfoService } from './services/userinfo.service';
import * as signalR from '@microsoft/signalr';

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
    private _tokenService: TokenService
  ) {}

    private connect(): void {
      this.connection = new signalR.HubConnectionBuilder()
        .withUrl(this._signalRURL + "usersHub", {
          skipNegotiation: true,
          transport: signalR.HttpTransportType.WebSockets,
          accessTokenFactory: () => {
            return this._tokenService.getJwtToken()!;
          }
        })
        .build();
        this.connection.start().catch(err => document.write(err));
    }

   private onAuthChange(res: boolean): void {
      if (res)
      {
        if (this.connection)
          this.connection?.stop().then(() => {
            this.connection = null;
            this.connect();
          });
        else
          this.connect();
        this._userInfoService.updateLastUser();
      }
      else
      {
        this.connection?.stop().then(() => this.connection = null);
      }
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

    this._accountService.authChanged
    .subscribe(res => {
      this.onAuthChange(res);
    })
    this._accountService.isUserAuthenticated().then(res => this.onAuthChange(res));
  }
}

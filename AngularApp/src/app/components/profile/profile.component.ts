import { Component, Inject, OnDestroy, OnInit } from '@angular/core';	
import { Title } from '@angular/platform-browser';
import { ActivatedRoute, Router } from '@angular/router';
import { UserService } from '../../services/user.service';
import { UserModel } from '../../shared/user/user.model';
import * as signalR from "@microsoft/signalr";
import { TokenService } from 'src/app/services/token.service';
import { AccountService } from 'src/app/services/account.service';
import { UserProfileReducedModel } from 'src/app/shared/user/user-profile-reduced.model';
import { ConnectionContainer } from 'src/app/shared/other/connection-container';
import { TokenGuardService } from 'src/app/services/token-guard.service';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss']
})
export class ProfileComponent implements OnInit, OnDestroy {
  user: UserModel = undefined!;
  id: number = undefined!;
  connection: signalR.HubConnection | null | undefined;
  isFirstEvent: boolean = true;

  public isAdmin: boolean = false;
  public userReduced: UserProfileReducedModel = undefined!;

  userBansConnectionContainer: ConnectionContainer = new ConnectionContainer();

  constructor(private _titleService: Title, private _route: ActivatedRoute, public usersService: UserService, private _router: Router, 
    @Inject('projectName') private _projectName: string, @Inject('signalRURL') private _signalRURL: string,
    private _tokenService: TokenService, private _accountService: AccountService, private _tokenGuardService: TokenGuardService) { 
    this.userBansConnectionContainer.connection = new signalR.HubConnectionBuilder()
    .withUrl(this._signalRURL + "userBansHub", {
      skipNegotiation: true,
      transport: signalR.HttpTransportType.WebSockets,
      accessTokenFactory: () => this._tokenGuardService.getOrRefreshToken()
    })
    .withAutomaticReconnect()
    .build();
  }

  private getConnectionId(): void
  {
    if (!this.userBansConnectionContainer.connection)
      return;
    this.userBansConnectionContainer.connection.invoke('GetConnectionId')
    .catch(error => console.error(error));
  }

  private subscribeToBans(): void
  {
    if (!this.userBansConnectionContainer.connection)
      return;
    this.userBansConnectionContainer.connection.invoke('StartListening', this.id)
    .catch(error => console.error(error));
  }

  ngOnInit(): void {
    this._route.paramMap.subscribe(params => {
      this.id = parseInt(params.get('id')!);
      this.userBansConnectionContainer.connection.on("ConnectionId", (result: string | null) => 
      {
        this.userBansConnectionContainer.id = result;
      });
      this.userBansConnectionContainer.connection.onreconnected(() => {
        this.getConnectionId();
        this.subscribeToBans();
      });
      this.usersService.getById(this.id)
      .subscribe({
        next: user => 
        {
          this.user = user;
          this.userReduced = 
          {
            ...user,
            id: this.id, 
            fullName: this.usersService.getFullName(this.user.firstName, this.user.lastName)
          }
          this.user.isOnline = false;
          this._titleService.setTitle(`${this._projectName} - ${this.usersService.getFullName(user.firstName, user.lastName)}'s profile`);
          this.connection = new signalR.HubConnectionBuilder()
          .withUrl(this._signalRURL + "usersHub", {
            skipNegotiation: true,
            transport: signalR.HttpTransportType.WebSockets,
          })
          .withAutomaticReconnect()
          .build();
          this.connection.onreconnected(() => this.startListening());
          this.connection.on("StatusChange", (data: boolean) => {
            if (this.isFirstEvent)
            {
              if (data || data != this.user.isOnline)
                this.isFirstEvent = false;
            }
            if (!data && !this.isFirstEvent)
              this.user.lastSeen = new Date().toString();
            this.user.isOnline = data;
          });
          this.connection.start().then(
            () => this.startListening()
          ).catch(err => document.write(err));
        },
        error: () => this._router.navigate(["**"], { skipLocationChange: true })
      });
    });
    this._accountService.authChanged
    .subscribe(() => {
      this.onAuthChange();
    })
    this._accountService.isUserAuthenticated().then(() => this.onAuthChange());
  }

  private onAuthChange(): void {
    this.isAdmin = this._tokenService.isAdmin();
    if (this.userBansConnectionContainer && this.userBansConnectionContainer.connection)
    {
      if (this.isAdmin && this.userBansConnectionContainer.connection.state == signalR.HubConnectionState.Disconnected)
      {
        this.userBansConnectionContainer.connection.start().then(() => {
            this.getConnectionId();
            this.subscribeToBans();
          });
      }
      else if (!this.isAdmin && this.userBansConnectionContainer.connection.state == signalR.HubConnectionState.Connected)
      {
        this.userBansConnectionContainer.connection.stop();
      }
    }
  }

  private startListening(): void {
    this.connection!.invoke('StartListening', this.id)
    .catch(error => console.error(error));
  }

  ngOnDestroy() {
    if (this.connection && this.connection.state == signalR.HubConnectionState.Connected)
      this.connection.stop().then(() => this.connection = null)
          .catch(error => console.error(error));
    else
      this.connection = null;
    if (this.userBansConnectionContainer.connection && this.userBansConnectionContainer.connection.state == signalR.HubConnectionState.Connected)
      this.userBansConnectionContainer.connection.stop().then(() => this.userBansConnectionContainer.connection = null!);
    else
      this.userBansConnectionContainer.connection = null!
  }
}

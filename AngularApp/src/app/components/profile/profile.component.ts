import { Component, Inject, OnInit } from '@angular/core';	
import { Title } from '@angular/platform-browser';
import { ActivatedRoute, Router } from '@angular/router';
import { UserService } from '../../services/user.service';
import { UserModel } from '../../shared/user/user.model';
import * as signalR from "@microsoft/signalr";

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss']
})
export class ProfileComponent implements OnInit {
  user: UserModel = undefined!;
  id: number = undefined!;
  connection: signalR.HubConnection | null | undefined;
  isFirstEvent: boolean = true;

  constructor(private _titleService: Title, private _route: ActivatedRoute, public usersService: UserService, private _router: Router, 
    @Inject('projectName') private _projectName: string, @Inject('signalRURL') private _signalRURL: string) { }

  ngOnInit(): void {
    this._route.paramMap.subscribe(params => {
      this.id = parseInt(params.get('id')!);
      this.usersService.getById(this.id)
      .subscribe({
        next: user => 
        {
          this.user = user;
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
            this.user.isOnline = data;
            if (this.isFirstEvent)
              this.isFirstEvent = false;
            else if (!data)
              this.user.lastSeen = new Date().toString();
          });
          this.connection.start().then(
            () => this.startListening()
          ).catch(err => document.write(err));
        },
        error: () => this._router.navigate(["**"], { skipLocationChange: true })
      });
    });
  }

  private startListening(): void {
    this.connection!.invoke('StartListening', this.id)
    .catch(error => console.error(error));
  }

  ngOnDestroy() {
    if (this.connection && this.connection.state == signalR.HubConnectionState.Connected)
    {
      this.connection.invoke('StopListening', this.id).then(() => this.connection!.stop())
          .catch(error => console.error(error));
    }
  }
}

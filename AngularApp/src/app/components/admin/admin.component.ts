import { Component, Inject, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { createNotWhitespaceValidator } from 'src/app/customvalidators';
import { UserService } from 'src/app/services/user.service';
import { ConnectionContainer } from 'src/app/shared/other/connection-container';
import { UserProfileReducedModel } from 'src/app/shared/user/user-profile-reduced.model';
import * as signalR from '@microsoft/signalr';
import { TokenGuardService } from 'src/app/services/token-guard.service';
import { ReplaySubject } from 'rxjs';
import { Pair } from 'src/app/shared/other/pair';

@Component({
  selector: 'app-admin',
  templateUrl: './admin.component.html',
  styleUrls: ['./admin.component.scss']
})
export class AdminComponent implements OnInit, OnDestroy {
  form: FormGroup = null!;
  @ViewChild('sform') loginFormDirective: any;
  formError: string | null | undefined;

  private readonly searchFormErrorMessage = 'Please, type at least three characters into the form above'; 

  search: FormControl = undefined!;

  searchOldValue: string | null | undefined;

  users: UserProfileReducedModel[] = undefined!

  loading: boolean = false;
  errorMessage: string | null | undefined;

  readonly minLength = 3;

  connectionContainer: ConnectionContainer = new ConnectionContainer();

  isConnectionReady: boolean = false;
  areUsersReady: boolean = false;

  everythingIsReady: ReplaySubject<Pair<boolean, boolean>> = new ReplaySubject<Pair<boolean, boolean>>(1);

  constructor(private _fb: FormBuilder, private _router: Router, private _route: ActivatedRoute, private _userService: UserService,
    @Inject('signalRURL') private _signalRURL: string, private _tokenGuardService: TokenGuardService) {
    this.createForm();
    this.connectionContainer.connection = new signalR.HubConnectionBuilder()
    .withUrl(this._signalRURL + "userBansHub", {
      skipNegotiation: true,
      transport: signalR.HttpTransportType.WebSockets,
      accessTokenFactory: () => this._tokenGuardService.getOrRefreshToken()
    })
    .withAutomaticReconnect()
    .build();
   }

  async ngOnInit(): Promise<void> {
    this.searchOldValue = this._route.snapshot.queryParams['search'];
    if (this.searchOldValue && this.searchOldValue.length >= this.minLength)
      this.getResults();
    this.everythingIsReady.subscribe(value =>
      {
        if (value.Item1 && value.Item2)
          this.subscribeToAll();
      });
    this.connectionContainer.connection.on("ConnectionId", (result: string | null) => 
    {
      this.connectionContainer.id = result;
    });
    this.connectionContainer.connection.onreconnected(() => {
      this.getConnectionId();
      this.subscribeToAll();
    });
    try {
      await this.connectionContainer.connection.start().then(() => {
        this.getConnectionId();
        this.isConnectionReady = true;
        this.sendReadyState();
      });
    } catch (err) {
      console.error(err);
    }
  }

  private sendReadyState(): void
  {
    this.everythingIsReady.next({
      Item1: this.isConnectionReady,
      Item2: this.areUsersReady
    });
  }

  private subscribeToAll()
  {
    if (!this.users || this.connectionContainer.connection.state != signalR.HubConnectionState.Connected)
      return;
    this.users.forEach(u => {
      this.connectionContainer.connection.invoke('StartListening', u.id)
      .catch(error => console.error(error));
    });
  }

  private getConnectionId()
  {
   this.connectionContainer.connection.invoke('GetConnectionId')
    .catch(error => console.error(error));
  }

  private getResults(): void
  {
    this.errorMessage = null;
    this.loading = true;
    this._userService.get(this.searchOldValue)
    .subscribe({
      next: result =>
      {
        this.loading = false;
        let oldUsers = this.users;
        this.users = result;
        if (!this.areUsersReady)
        {
          this.areUsersReady = true;
          this.sendReadyState();
        }
        else
        {
          this.subscribeToAll();
          if (oldUsers)
          {
            oldUsers = oldUsers.filter(u => this.users.findIndex(newu => newu.id == u.id) == -1);
            oldUsers.forEach(u =>
              {
                this.connectionContainer.connection.invoke('StopListening', u.id)
                .catch(error => console.error(error));
              });
          }
        }
      },
      error: error =>
      {
        this.loading = false;
        const message = `An error occured: ${error.status} - ${error.statusText || ''}\n${JSON.stringify(error.error)}`;
        this.errorMessage = message;
        console.error(message);
      }
    });
  }

  createForm() {
    this.search = new FormControl('', [Validators.required, createNotWhitespaceValidator(), Validators.minLength(3)]);
    this.form = this._fb.group({
      search: this.search
    });
  }

  public onSubmit(): void
  {
    if (!this.form.valid)
    {
      this.formError = this.searchFormErrorMessage;
      return;
    }
    this.formError = null;
    this.searchOldValue = this.search.value;
    this._router.navigate(['/admin'], { queryParams: { search: this.searchOldValue } });
    this.getResults();
  }

  ngOnDestroy(): void {
      if (this.connectionContainer.connection && this.connectionContainer.connection.state == signalR.HubConnectionState.Connected)
        this.connectionContainer.connection.stop().then(() => this.connectionContainer.connection = null!);
      else
        this.connectionContainer.connection = null!
  }
}

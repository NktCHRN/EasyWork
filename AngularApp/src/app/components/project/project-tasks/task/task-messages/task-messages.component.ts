import { Component, Inject, Input, OnDestroy, OnInit, Output, QueryList, ViewChild, ViewChildren } from '@angular/core';
import { TaskService } from 'src/app/services/task.service';
import { TokenService } from 'src/app/services/token.service';
import { MessageModel } from 'src/app/shared/message/message.model';
import { EventEmitter } from '@angular/core';
import { UserReducedModel } from 'src/app/shared/user/user-reduced.model';
import { UserInfoService } from 'src/app/services/userinfo.service'
import { ProjectService } from 'src/app/services/project.service'
import { UserOnProjectReducedModel } from 'src/app/shared/project/user-on-project/user-on-project-reduced.model';
import { UserOnProjectRole } from 'src/app/shared/project/user-on-project/role/user-on-project-role';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { createNotWhitespaceValidator } from 'src/app/customvalidators';
import { AddMessageModel } from 'src/app/shared/message/add-message.model';
import { TaskMessageComponent } from './task-message/task-message.component';
import { ConnectionContainer } from 'src/app/shared/other/connection-container';
import { TokenGuardService } from 'src/app/services/token-guard.service';
import * as signalR from '@microsoft/signalr';

@Component({
  selector: 'app-task-messages',
  templateUrl: './task-messages.component.html',
  styleUrls: ['./task-messages.component.scss']
})
export class TaskMessagesComponent implements OnInit, OnDestroy {
  @Input() taskId: number = undefined!;
  @Input() projectId: number = undefined!;
  messages: MessageModel[] = undefined!;
  loading: boolean = true;
  errorMessage: string | null | undefined;
  @Output() closeDialog = new EventEmitter();
  myId: number;
  me: UserReducedModel = undefined!;
  meOnProject: UserOnProjectReducedModel | null | undefined;
  userOnProjectRoles = UserOnProjectRole;

  @ViewChildren(TaskMessageComponent) viewMessages: QueryList<TaskMessageComponent> = undefined!;

  @Output() addedMessage = new EventEmitter();
  @Output() deletedMessage = new EventEmitter();

  @Output() switchedToLoading = new EventEmitter();
  @Output() switchedToSuccess = new EventEmitter();
  @Output() switchedToFail = new EventEmitter();

  form: FormGroup = null!;
  @ViewChild('aform') formDirective: any;

  formErrors : any = {
    'text': '',
  };

  validationMessages : any = {
    'text': {
      'required':      'Name is required.',
      'notWhitespace':      'Name cannot be whitespace-only.',
      'maxlength':     'Name cannot be more than 2000 characters long.',
    },
  };

  @Input() tasksConnectionContainer: ConnectionContainer = new ConnectionContainer();
  connectionContainer: ConnectionContainer = new ConnectionContainer();

  constructor(private _tokenService: TokenService, private _taskService: TaskService,
    private _userInfoService: UserInfoService, private _projectService: ProjectService, private _fb: FormBuilder,
    @Inject('signalRURL') private _signalRURL: string, private _tokenGuardService: TokenGuardService) {
      this.myId = this._tokenService.getMyId()!;
      this.connectionContainer.connection = new signalR.HubConnectionBuilder()
      .withUrl(this._signalRURL + "messagesHub", {
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets,
        accessTokenFactory: () => this._tokenGuardService.getOrRefreshToken()
      })
      .withAutomaticReconnect()
      .build();
      this.createForm();
  }

  ngOnInit(): void {
    this.connectionContainer.connection.onreconnected(() =>
    {
      this.getConnectionId();
      this.subscribeToAll();
    });
    this.connectionContainer.connection.on("ConnectionId", (result: string | null) => 
    {
      this.connectionContainer.id = result;
    });
    this._taskService.getMessages(this.taskId)
    .subscribe({
      next: async result => {
        this.messages = result;
        this.loading = false;
        try {
          await this.connectionContainer.connection.start().then(() => {
            this.getConnectionId();
            this.subscribeToAll();
          });
        } catch (err) {
          console.error(err);
        }
      },
      error: error => {
        this.errorMessage = `$Messages have not been loaded. Error: ${error.status} - ${error.statusText || ''}\n${JSON.stringify(error.error)}`;
        this.loading = false;
      }
    });
    this._userInfoService.lastUser.subscribe(user => this.me = user!);
    this._projectService.getMeAsProjectUser(this.projectId)
    .subscribe(user => this.meOnProject = user);
    this.tasksConnectionContainer.connection.on("AddedMessage", (taskId: number, model: MessageModel) => {
      if (taskId == this.taskId && this.messages)
        this.addMessage(model);
    });
    this.tasksConnectionContainer.connection.on("DeletedMessage", (taskId: number, messageId: number) =>
    {
      if (taskId == this.taskId && this.messages)
        this.deleteMessage(messageId);
    });
  }

  onCloseDialog(): void {
    this.closeDialog.emit();
  }

  createForm() {
    this.form = this._fb.group({
      text: ['', [Validators.required, createNotWhitespaceValidator(), Validators.maxLength(2000)] ]
    });

    this.form.valueChanges
    .subscribe(data => this.onValueChanged(data));

    this.onValueChanged();
  }

  onValueChanged(data?: any) {
    if (!this.form)
      return;
    const form = this.form;
    for (const field in this.formErrors)
    {
      if (this.formErrors.hasOwnProperty(field)) {
        this.formErrors[field] = '';
        const control = form.get(field);
        if (control && control.dirty && !control.valid) {
          const messages = this.validationMessages[field];
          for (const key in control.errors) {
            if (control.errors.hasOwnProperty(key)) {
              this.formErrors[field] += messages[key] + ' ';
            }
          }
        }
      }
    }
  }

  private addMessage(model: MessageModel): void 
  {
    this.messages.push(model);
    this.subscribeToMessage(model.id);
  }

  private getConnectionId()
  {
   this.connectionContainer.connection.invoke('GetConnectionId')
    .catch(error => console.error(error));
  }

  private subscribeToAll(): void
  {
    if (!this.messages)
      return;
    this.messages.forEach(t => this.subscribeToMessage(t.id));
  }

  private subscribeToMessage(id: number): void
  {
    this.connectionContainer.connection.invoke('StartListening', id)
    .catch(error => console.error(error));
  }

  private unsubscribeFromMessage(id: number): void
  {
    this.connectionContainer.connection.invoke('StopListening', id)
    .catch(error => console.error(error));
  }

  onSubmit() {
    if (this.form.valid)
    {
      this.switchedToLoading.emit();
      const model: AddMessageModel = this.form.value;
      this._taskService.addMessage(this.tasksConnectionContainer.id, this.taskId, model)
      .subscribe({
        next: result => {
          this.switchedToSuccess.emit();
          this.addedMessage.emit();
          this.addMessage(result);
          this.form.reset();
          this.formDirective.resetForm();
        },
        error: error => {
          this.switchedToFail.emit();
          console.log(error);
        }
      });
    }
  }

  onDeletedMessage(id: number) {
    this.deleteMessage(id);
    this.deletedMessage.emit();
  }

  private deleteMessage(id: number): void
  {
    this.unsubscribeFromMessage(id);
    const index = this.messages.findIndex(m => m.id == id);
    if (index != -1)
      this.messages.splice(index, 1);
  }

  closeOther(callerId: number) {
    this.viewMessages?.forEach(viewMessage => {
      if (viewMessage.message?.id != callerId)
        viewMessage.switchToShowMode();
    });
  }

  ngOnDestroy(): void {
    if (this.connectionContainer.connection && this.connectionContainer.connection.state == signalR.HubConnectionState.Connected)
      this.connectionContainer.connection.stop().then(() => this.connectionContainer.connection = null!);
    else
      this.connectionContainer.connection = null!
  }
}

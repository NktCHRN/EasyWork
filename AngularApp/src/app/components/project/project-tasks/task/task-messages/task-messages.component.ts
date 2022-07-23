import { Component, Input, OnInit, Output, QueryList, ViewChild, ViewChildren } from '@angular/core';
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

@Component({
  selector: 'app-task-messages',
  templateUrl: './task-messages.component.html',
  styleUrls: ['./task-messages.component.scss']
})
export class TaskMessagesComponent implements OnInit {
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

  constructor(private _tokenService: TokenService, private _taskService: TaskService,
    private _userInfoService: UserInfoService, private _projectService: ProjectService, private _fb: FormBuilder) {
      this.myId = this._tokenService.getMyId()!;
      this.createForm();
  }

  ngOnInit(): void {
    this._taskService.getMessages(this.taskId)
    .subscribe({
      next: result => {
        this.messages = result;
        this.loading = false;
      },
      error: error => {
        this.errorMessage = `$Messages have not been loaded. Error: ${error.status} - ${error.statusText || ''}\n${JSON.stringify(error.error)}`;
        this.loading = false;
      }
    });
    this._userInfoService.lastUser.subscribe(user => this.me = user!);
    this._projectService.getMeAsProjectUser(this.projectId)
    .subscribe(user => this.meOnProject = user);
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

  onSubmit() {
    if (this.form.valid)
    {
      this.switchedToLoading.emit();
      const model: AddMessageModel = this.form.value;
      this._taskService.addMessage(this.taskId, model)
      .subscribe({
        next: result => {
          this.switchedToSuccess.emit();
          this.addedMessage.emit();
          this.messages.push(result);
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
    const index = this.messages.findIndex(m => m.id == id);
    if (index != -1)
      this.messages.splice(index, 1);
    this.deletedMessage.emit();
  }

  closeOther(callerId: number) {
    this.viewMessages?.forEach(viewMessage => {
      if (viewMessage.message?.id != callerId)
        viewMessage.switchToShowMode();
    });
  }
}

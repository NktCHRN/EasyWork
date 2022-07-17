import { Component, ElementRef, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { createNotWhitespaceValidator } from 'src/app/customvalidators';
import { MessageService } from 'src/app/services/message.service';
import { TokenService } from 'src/app/services/token.service';
import { AddMessageModel } from 'src/app/shared/message/add-message.model';
import { MessagePageMode } from 'src/app/shared/message/message-page-mode';
import { MessageModel } from 'src/app/shared/message/message.model';
import { UserOnProjectRole } from 'src/app/shared/project/user-on-project/role/user-on-project-role';
import { UserOnProjectReducedModel } from 'src/app/shared/project/user-on-project/user-on-project-reduced.model';
import { TaskMessageDeleteComponent } from '../task-message-delete/task-message-delete.component';

@Component({
  selector: 'app-task-message',
  templateUrl: './task-message.component.html',
  styleUrls: ['./task-message.component.scss']
})
export class TaskMessageComponent implements OnInit {
  @Input() message: MessageModel = undefined!;
  myId: number;
  @Input() meOnProject: UserOnProjectReducedModel | null | undefined;
  userOnProjectRoles = UserOnProjectRole;
  @Output() closeDialog = new EventEmitter();
  @Output() deletedMessage: EventEmitter<number> = new EventEmitter<number>();
  modes = MessagePageMode;
  mode: MessagePageMode = MessagePageMode.Show;
  @Output() closeOther: EventEmitter<number> = new EventEmitter<number>();

  @Output() switchedToLoading = new EventEmitter();
  @Output() switchedToSuccess = new EventEmitter();
  @Output() switchedToFail = new EventEmitter();

  form: FormGroup = null!;
  @ViewChild('uform') formDirective: any;

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

  constructor(private _tokenService: TokenService, private _dialog: MatDialog, private _fb: FormBuilder, private _messageService: MessageService) {
    this.myId = this._tokenService.getMyId()!;
    this.createForm();
  }

  ngOnInit(): void {
  }

  openDeleteDialog(): void {
    const dialogRef = this._dialog.open(TaskMessageDeleteComponent, {
      panelClass: "mini-dialog-responsive",
      data: this.message.id
    });
    dialogRef.afterClosed()
    .subscribe(() => {
      if (dialogRef.componentInstance.success)
        this.deletedMessage.emit(this.message.id);
    });
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
      this._messageService.update(this._tokenService.getJwtToken()!, this.message.id, model)
      .subscribe({
        next: () => {
          this.switchedToSuccess.emit();
          this.message.text = model.text;
          this.form.reset();
          this.switchToShowMode();
        },
        error: error => {
          this.switchedToFail.emit();
          console.log(error);
        }
      });
    }
  }

  switchToEditMode(): void
  {
    this.closeOther.emit(this.message.id);
    this.mode = this.modes.Edit;
    this.form.controls['text'].setValue(this.message.text);
  }

  switchToShowMode(): void {
    this.mode = this.modes.Show;
  }
}

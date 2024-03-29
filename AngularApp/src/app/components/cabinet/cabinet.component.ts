import { Component, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AccountService } from '../../services/account.service';
import { UserInfoService } from '../../services/userinfo.service';
import { createNotWhitespaceValidator } from '../../customvalidators';
import { UpdateUserModel } from '../../shared/user/update-user.model';
import { UserCabinetModel } from '../../shared/user/cabinet/user-cabinet.model';
import { AvatarBaseComponent } from './avatar/avatar-base/avatar-base.component';
import { UserReducedModel } from 'src/app/shared/user/user-reduced.model';

@Component({
  selector: 'app-cabinet',
  templateUrl: './cabinet.component.html',
  styleUrls: ['./cabinet.component.scss']
})
export class CabinetComponent implements OnInit {

  miniUser: UserReducedModel = new UserReducedModel();
  user: UserCabinetModel = new UserCabinetModel();
  updateUser: UpdateUserModel = new UpdateUserModel();
  form: FormGroup = null!;
  @ViewChild('cform') formDirective: any;

  formErrors : any = {
    'firstName': '',
    'lastName': '',
    'phoneNumber': ''
  };

  private validationMessages : any = {
    'firstName': {
      'required':      'First Name is required.',
      'notWhitespace':      'First Name cannot be whitespace-only.',
      'maxlength':     'First name cannot be more than 50 characters long.'
    },
    'lastName': {
      'maxlength':     'Last name cannot be more than 50 characters long.',
    },
    'phoneNumber':
    {
      'minlength':     'Phone number cannot be less than 8 characters long.',
      'maxlength':     'Phone number cannot be more than 15 characters long.',
    }
  };

  constructor(private _fb: FormBuilder, private _userInfoService: UserInfoService, private _accountService: AccountService,
    private _snackBar: MatSnackBar, private _dialog: MatDialog) { 
    this.createForm();
  }

  ngOnInit(): void {
    this._accountService.get()
    .subscribe({
      next: result => 
      {
        this.user = result;
        this.user.registrationDate = new Date(this.user.registrationDate).toString();       // conversion from UTC to local
        this.form.controls['firstName'].setValue(this.user.firstName);
        this.form.controls['lastName'].setValue(this.user.lastName);
        this.form.controls['phoneNumber'].setValue(this.user.phoneNumber);
      },
      error: error => this._snackBar.open(error, "Close", {duration: 5000})
    });
    this._userInfoService.lastUser.subscribe(user => this.miniUser = user!);
  }

  public changeNumber(event: any)
  {
    let target = event.target;
    target.value = target.value.replace(/[^0-9.]/g, '');
  }

  createForm() {
    this.form = this._fb.group({
      firstName: ['', [Validators.required, createNotWhitespaceValidator(), Validators.maxLength(50)] ],
      lastName: ['', [Validators.maxLength(50)] ],
      phoneNumber: ['', [Validators.minLength(8), Validators.maxLength(15)]]
    });

    this.form.valueChanges
    .subscribe(data => this.onValueChanged(data));
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

  onSubmit(event: any) {
    const callerName:string = event.target.attributes.getNamedItem('ng-reflect-name').value;
    type ObjectKey = keyof typeof this.user;
    const callerNameAsKey = callerName as ObjectKey;
    if ((this.user[callerNameAsKey]) != event.target.value && this.form.valid)
    {
      this.updateUser = this.form.value;
    this._accountService.update(this.updateUser!)
    .subscribe({
      next: () => {
        this._snackBar.open("Updated successfully", "Close", {
          duration: 1000,
          panelClass: "snackbar-orange"
        });
        this._userInfoService.updateLastUser();
        this.user = {
          ...this.user,
          ...this.updateUser
        };
      },
      error: error => { 
        this._snackBar.open(`The user was not updated: ${error.status} - ${error.statusText || ''}\n${JSON.stringify(error.error)}`, "Close", {
          duration: 5000,
        });
      }
    });
  }
  }

  openAvatarDialog() : void
  {
    this._dialog.open(AvatarBaseComponent, {
      panelClass: "dialog-responsive"
    });
  }
}

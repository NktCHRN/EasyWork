import { Component, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute } from '@angular/router';
import { AccountService } from '../services/account.service';
import { createHasLowerCaseValidator, createHasNumbericValidator, createHasUpperCaseValidator, createIsEqualToValidator } from '../shared/customvalidators';
import { ResetPasswordModel } from '../shared/reset-password.model';

@Component({
  selector: 'app-reset-password',
  templateUrl: './reset-password.component.html',
  styleUrls: ['./reset-password.component.scss']
})
export class ResetPasswordComponent implements OnInit {
  hide = true;
  form: FormGroup = null!;
  @ViewChild('rform') formDirective: any;
  loading: boolean = false;
  success: boolean = false;
  private _model: ResetPasswordModel = new ResetPasswordModel();

  formErrors : any = {
    'password': '',
    'passwordConfirm': ''
  };

  validationMessages : any = {
    'password': {
      'required':      'New password is required.',
      'minlength':     'New password must be at least 8 characters long.',
      'hasNumberic':         'New password must contain at least one number.',
      'hasLowerCase':         'New password must contain at least one lowercase letter.',
      'hasUpperCase':         'New password must contain at least one uppercase letter.'
    },
    'passwordConfirm': {
      'required':      'Password confirm is required.',
      'matches': 'Passwords are not matching'
    }
  };

  constructor(private _fb: FormBuilder, 
    private _accountService: AccountService,
    private _snackBar: MatSnackBar,
    private _route: ActivatedRoute) { 
      this.createForm();
    }

  ngOnInit(): void {
    this._model.token = this._route.snapshot.queryParams['token'];
    this._model.email = this._route.snapshot.queryParams['email'];
  }

  createForm() {
    this.form = this._fb.group({
      password: ['', [Validators.required, Validators.minLength(8), createHasNumbericValidator(), createHasLowerCaseValidator(), createHasUpperCaseValidator()]],
      passwordConfirm: ['', [Validators.required, createIsEqualToValidator('password')]]
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
    this.loading = true;
    this._model.password = this.form.get('password')?.value;
    this._model.passwordConfirm = this.form.get('passwordConfirm')?.value;
    this._accountService.resetPassword(this._model)
    .subscribe({
      next: () => {
        this.loading = false;
        this.success = true;   
      },
      error: error => { 
        this._model.password = '';
        this._model.passwordConfirm = '';
        this.loading = false;
        this._snackBar.open(`${error.status} - ${error.statusText || ''}\n${error.error}`, "Close", {
          panelClass: ['pre-wrap']
      });
      }
    });
  }

}

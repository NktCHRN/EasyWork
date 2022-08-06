import { Component, Inject, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AccountService } from '../../services/account.service';
import { createHasLowerCaseValidator, createHasNumbericValidator, createHasUpperCaseValidator, createIsDusplicateValidator, createIsEqualToValidator, createNotWhitespaceValidator } from '../../customvalidators';
import { RegisterUserModel } from '../../shared/user/register-user.model';
import {MatSnackBar} from '@angular/material/snack-bar';
import { SocialUser } from 'angularx-social-login';
import { ExternalAuthModel } from '../../shared/token/external-auth.model';
import { Router } from '@angular/router';
import { BooleanContainer } from '../../shared/other/booleancontainer';
import { first } from 'rxjs/operators';

@Component({
  selector: 'app-registration',
  templateUrl: './registration.component.html',
  styleUrls: ['./registration.component.scss']
})
export class RegistrationComponent implements OnInit {

  hide = true;
  registrationForm: FormGroup = null!;
  @ViewChild('rform') registrationFormDirective: any;
  loading: boolean = false;
  registerUser: RegisterUserModel | null | undefined;
  success: boolean = false;
  isEmailDuplicate: BooleanContainer = new BooleanContainer();

  formErrors : any = {
    'firstName': '',
    'lastName': '',
    'email': '',
    'password': '',
    'passwordConfirm': ''
  };

  validationMessages : any = {
    'firstName': {
      'required':      'First Name is required.',
      'notWhitespace':      'First Name cannot be whitespace-only.',
      'maxlength':     'First name cannot be more than 50 characters long.'
    },
    'lastName': {
      'maxlength':     'Last name cannot be more than 50 characters long.',
    },
    'email': {
      'required':      'Email is required.',
      'email':         'Email not in valid format.',
      'notDuplicate' : 'This email is already taken'
    },
    'password': {
      'required':      'Password is required.',
      'minlength':     'Password must be at least 8 characters long.',
      'hasNumberic':         'Password must contain at least one number.',
      'hasLowerCase':         'Password must contain at least one lowercase letter.',
      'hasUpperCase':         'Password must contain at least one uppercase letter.'
    },
    'passwordConfirm': {
      'required':      'Password confirm is required.',
      'matches': 'Passwords are not matching'
    }
  };

  constructor(private _fb: FormBuilder, 
    private _accountService: AccountService,
    private _snackBar: MatSnackBar,
    @Inject('emailConfirmationURL') private _emailConfirmationURL: string,
    private _router: Router) {
      this.createForm();
     }

  ngOnInit(): void {
  }

  createForm() {
    this.registrationForm = this._fb.group({
      firstName: ['', [Validators.required, createNotWhitespaceValidator(), Validators.maxLength(50)] ],
      lastName: ['', [Validators.maxLength(50)] ],
      email: ['', [Validators.required, Validators.email, createIsDusplicateValidator(this.isEmailDuplicate)] ],
      password: ['', [Validators.required, Validators.minLength(8), createHasNumbericValidator(), createHasLowerCaseValidator(), createHasUpperCaseValidator()]],
      passwordConfirm: ['', [Validators.required, createIsEqualToValidator('password')]]
    });

    this.registrationForm.valueChanges
    .subscribe(data => this.onValueChanged(data));

    this.onValueChanged();
  }

  onValueChanged(data?: any) {
    if (!this.registrationForm)
      return;
    const form = this.registrationForm;
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
    this.registerUser = this.registrationForm.value;
    this.registerUser!.clientURI = this._emailConfirmationURL;
    this._accountService.register(this.registerUser!)
    .subscribe({
      next: () => {
        this.loading = false;
        this.success = true;   
      },
      error: error => { 
        this.registerUser = null; 
        this.loading = false;
        let found: any = error.error.find((element: { code: string; }) => element.code == 'DuplicateEmail');
        if (found)
        {
          this.validationMessages.email.notDuplicate = found.description;
          this.isEmailDuplicate.value = true;
          let email = this.registrationForm.get('email');
          email?.markAsTouched();
          email?.markAsDirty();
          email?.updateValueAndValidity();
          this.onValueChanged();
          this.registrationForm.get('email')?.valueChanges.pipe(first())
          .subscribe(() => 
          {
              this.isEmailDuplicate.value = false;
              email?.updateValueAndValidity();
          })
        }
        else
          this._snackBar.open(`${error.status} - ${error.statusText || ''}\n${JSON.stringify(error.error)}`, "Close");
      }
    });
  }

  public externalLogin = () => {
    this._accountService.signInWithGoogle()
    .then(res => {
      const user: SocialUser = { ...res };
      const externalAuth: ExternalAuthModel = {
        provider: user.provider,
        idToken: user.idToken
      }
      this.validateExternalAuth(externalAuth);
    }, error => this._snackBar.open("Google Auth: " + error.statusText, "Close", {duration: 5000}))
  }

  private validateExternalAuth(externalAuth: ExternalAuthModel) {
    this._accountService.externalLogin(externalAuth)
      .subscribe({
        next: response => {
        const token = response.token!.accessToken;
        const refreshToken = response.token!.refreshToken;
        localStorage.setItem("jwt", token);
        localStorage.setItem("refreshToken", refreshToken); 
        this._accountService.sendAuthStateChangeNotification(response.isAuthSuccessful);
        this._router.navigate(["/cabinet"]);
      },
      error: error => {
        this._snackBar.open("Google Auth: " + error.statusText, "Close", {duration: 5000});
        this._accountService.signOutExternal();
      }
    });
  }
}

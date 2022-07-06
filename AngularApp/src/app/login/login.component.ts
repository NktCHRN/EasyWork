import { Component, Inject, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { SocialUser } from 'angularx-social-login';
import { BannedComponent } from '../banned/banned.component';
import { AccountService } from '../services/account.service';
import { AuthenticatedResponse } from '../shared/authenticatedresponse';
import { ExternalAuthModel } from '../shared/externalauthmodel';
import { ForgotPasswordModel } from '../shared/forgot-password.model';
import { LoginModel } from '../shared/loginmodel';
import { ResendEmailConfirmation } from '../shared/resendemailconfirmation';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {
  hide = true;
  loginForm: FormGroup = null!;
  @ViewChild('lform') loginFormDirective: any;
  loading: boolean = false;
  loginUser: LoginModel = {email:'', password:''};
  errorMessage: string | undefined | null;
  buttonEmail: string | undefined | null;
  disableResendEmailButton: boolean = false;
  showConfirmEmailButton: boolean = false;
  showResetPasswordButton: boolean = false;
  disableResetPasswordButton: boolean = false;
  private readonly _returnUrl: string = "/cabinet";

  formErrors : any = {
    'email': '',
    'password': '',
  };

  validationMessages : any = {
    'email': {
      'required':      'Email is required.',
      'email':         'Email not in valid format.'
    },
    'password': {
      'required':      'Password is required.'
    },
  };


  constructor(private _router: Router, private _fb: FormBuilder, 
    private _accountService: AccountService,
    @Inject('appURL') private _appURL:string,
    @Inject('emailConfirmationURL') private _emailConfirmationURL: string,
    private _snackBar: MatSnackBar, private _dialog: MatDialog) { 
      this.createForm();
    }
  ngOnInit(): void {
    
  }
  
  createForm() {
    this.loginForm = this._fb.group({
      email: ['', [Validators.required, Validators.email] ],
      password: ['', [Validators.required]]
    });

    this.loginForm.valueChanges
    .subscribe(data => this.onValueChanged(data));

    this.onValueChanged();
  }

  onValueChanged(data?: any) {
    if (!this.loginForm)
      return;
    const form = this.loginForm;
    for (const field in this.formErrors)
    {
      if (this.formErrors.hasOwnProperty(field)) {
        // clear previous error message (if any)
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
    if (!this.loginForm.valid)
    return;
    if (this.buttonEmail != this.loginForm.get('email')?.value)
      this.disableResetPasswordButton = false;
    this.showConfirmEmailButton = false;
    this.showResetPasswordButton = false;
    this.buttonEmail = undefined;
    this.errorMessage = undefined;
    this.loading = true;
    this.loginUser = this.loginForm.value;
    this._accountService.login(this.loginUser!)
    .subscribe({
      next: (response: AuthenticatedResponse) => {
        this.loading = false;
        const token = response.token!.accessToken;
        const refreshToken = response.token!.refreshToken;
        localStorage.setItem("jwt", token);
        localStorage.setItem("refreshToken", refreshToken); 
        this._accountService.sendAuthStateChangeNotification(response.isAuthSuccessful);
        this._router.navigate([this._returnUrl]);
      },
      error: error => { 
        this.loading = false;
        if (error?.error?.errorMessage == "You are banned from this website")
          error.message = "You are banned from this website";
          switch (error.message)
          {
            case "Please, confirm your email first": {
              this.buttonEmail = this.loginUser.email;
              this.showConfirmEmailButton = true;
              break;
            }
            case "Wrong email or password": {
              this.errorMessage = error;
              this.buttonEmail = this.loginUser.email;
              this.showResetPasswordButton = true;
              break;
            }
            case "You are banned from this website": {
              this.errorMessage = `${error.message}`;
              this._dialog.open(BannedComponent, {
                panelClass: "dialog-responsive",
                data: error.error.errorDetails
              });
              break;
            }
            default: {
              this.errorMessage = error;
              break;
            }
          }
      }
    });
  }

  public resendEmail()
  {
    this.disableResendEmailButton = true;
    let model = new ResendEmailConfirmation();
    model.email = this.buttonEmail!;
    model.clientURI = this._emailConfirmationURL;
    this._accountService.resendEmail(model)
    .subscribe({
      next: () => 
      {
        this._snackBar.open("The email has been sent once more successfully", "Close", {duration: 5000})
      },
      error: err => {
        this._snackBar.open("Error: " + err.error, "Close", {duration: 5000})
      }
    })
  }

  public resetPassword()
  {
    this.disableResetPasswordButton = true;
    let model = new ForgotPasswordModel();
    model.email = this.buttonEmail!;
    model.clientURI = this._appURL + "resetpassword";
    this._accountService.forgotPassword(model)
    .subscribe({
      next: () => 
      {
        this._snackBar.open("Please, follow the instructions in the email we have just sent you in order to reset your password", "Close", {duration: 5000})
      },
      error: err => {
        this._snackBar.open("Error: " + err.error, "Close", {duration: 5000})
      }
    })
  }

  public externalLogin = () => {
    this.errorMessage = undefined;
    this._accountService.signInWithGoogle()
    .then(res => {
      const user: SocialUser = { ...res };
      const externalAuth: ExternalAuthModel = {
        provider: user.provider,
        idToken: user.idToken
      }
      this.validateExternalAuth(externalAuth);
    }, error => this.errorMessage = error.statusText)
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
        this._router.navigate([this._returnUrl]);
      },
      error: error => {
        if (error?.error?.errorMessage == "You are banned from this website"){
          this.errorMessage = `${error.error.errorMessage}`;
          this._dialog.open(BannedComponent, {
            panelClass: "dialog-responsive",
            data: error.error.errorDetails
          });
        }
        else
        {
          this.errorMessage = error.statusText;
          this._accountService.signOutExternal();
        }
      }
    });
  }
}

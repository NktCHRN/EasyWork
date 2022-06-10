import { Component, Inject, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { SocialUser } from 'angularx-social-login';
import { AccountService } from '../services/account.service';
import { AuthenticatedResponse } from '../shared/authenticatedresponse';
import { ExternalAuthModel } from '../shared/externalauthmodel';
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
  confirmEmail: string | undefined | null;
  disableButton: boolean = false;
  readonly returnUrl: string = "/cabinet";

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


  constructor(private router: Router, private fb: FormBuilder, 
    private accountService: AccountService,
    @Inject('confirmEmailURI') public confirmEmailURI:string,
    private _snackBar: MatSnackBar) { 
      this.createForm();
    }
  ngOnInit(): void {
    
  }
  
  createForm() {
    this.loginForm = this.fb.group({
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
    this.confirmEmail = undefined;
    this.errorMessage = undefined;
    this.loading = true;
    this.loginUser = this.loginForm.value;
    this.accountService.login(this.loginUser!)
    .subscribe({
      next: (response: AuthenticatedResponse) => {
        this.loading = false;
        const token = response.token!.accessToken;
        const refreshToken = response.token!.refreshToken;
        localStorage.setItem("jwt", token);
        localStorage.setItem("refreshToken", refreshToken); 
        this.accountService.sendAuthStateChangeNotification(response.isAuthSuccessful);
        this.router.navigate([this.returnUrl]);
      },
      error: error => { 
        this.loading = false;
        if (error.message != "Please, confirm your email first")
          this.errorMessage = error;
        else
        {
          this.confirmEmail = this.loginUser.email;
        }
      }
    });
  }

  public resendEmail()
  {
    this.disableButton = true;
    let model = new ResendEmailConfirmation();
    model.email = this.confirmEmail!;
    model.clientURI = this.confirmEmailURI;
    this.accountService.resendEmail(model)
    .subscribe({
      next: () => 
      {
        this._snackBar.open("The email has been sent once more successfully", "Close", {duration: 5000})
      },
      error: err => {
        this._snackBar.open(err, "Close", {duration: 5000})
      }
    })
  }

  public externalLogin = () => {
    this.errorMessage = undefined;
    this.accountService.signInWithGoogle()
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
    this.accountService.externalLogin(externalAuth)
      .subscribe({
        next: response => {
        const token = response.token!.accessToken;
        const refreshToken = response.token!.refreshToken;
        localStorage.setItem("jwt", token);
        localStorage.setItem("refreshToken", refreshToken); 
        this.accountService.sendAuthStateChangeNotification(response.isAuthSuccessful);
        this.router.navigate([this.returnUrl]);
      },
      error: error => {
        this.errorMessage = error.statusText;
        this.accountService.signOutExternal();
      }
    });
  }
}

import { Component, Inject, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AccountService } from '../services/account.service';
import { createHasLowerCaseValidator, createHasNumbericValidator, createHasUpperCaseValidator, createIsEqualToValidator, createNotWhitespaceValidator } from '../shared/customvalidators';
import { RegisterUser } from '../shared/registeruser';
import {MatSnackBar} from '@angular/material/snack-bar';

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
  registerUser: RegisterUser | null | undefined;
  success: boolean = false

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
      'email':         'Email not in valid format.'
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


  constructor(private fb: FormBuilder, 
    private accountService: AccountService,
    private _snackBar: MatSnackBar,
    @Inject('confirmEmailURI') public confirmEmailURI:string) {
      this.createForm();
     }

  ngOnInit(): void {
  }

  createForm() {
    this.registrationForm = this.fb.group({
      firstName: ['', [Validators.required, createNotWhitespaceValidator(), Validators.maxLength(50)] ],
      lastName: ['', [Validators.maxLength(50)] ],
      email: ['', [Validators.required, Validators.email] ],
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
    this.loading = true;
    this.registerUser = this.registrationForm.value;
    this.registerUser!.clientURI = this.confirmEmailURI;
    console.log(this.registerUser?.clientURI)
    this.accountService.register(this.registerUser!)
    .subscribe({
      next: () => {
        this.loading = false;
        this.success = true;   
      },
      error: errmess => { 
        this.registerUser = null; 
        this.loading = false;
        this._snackBar.open(errmess, "Close")
      }
    });
  }

}

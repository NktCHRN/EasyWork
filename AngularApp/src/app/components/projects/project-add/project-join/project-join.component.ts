import { Component, EventEmitter, OnInit, Output, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { InvitesService } from 'src/app/services/invites.service';
import { TokenService } from 'src/app/services/token.service';
import { BooleanContainer } from 'src/app/shared/other/booleancontainer';
import { createIsServerErrorValidator, createNotWhitespaceValidator } from 'src/app/customvalidators';
import { first } from 'rxjs';

@Component({
  selector: 'app-project-join',
  templateUrl: './project-join.component.html',
  styleUrls: ['./project-join.component.scss']
})
export class ProjectJoinComponent implements OnInit {
  loading: boolean = false;
  form: FormGroup = null!;
  @ViewChild('jform') formDirective: any;
  isServerError: BooleanContainer = new BooleanContainer();
  @Output() closeOuter = new EventEmitter();

  formErrors : any = {
    'inviteCode': '',
  };

  validationMessages : any = {
    'inviteCode': {
      'required':      'Invite code is required.',
      'notWhitespace':      'Invite code cannot be whitespace-only.',
      'maxlength':     'Too big invite code',
      'notServerError' : 'Server error'
    },
  };

  constructor(private _fb: FormBuilder,
    private _router: Router,
    private _invitesService: InvitesService,
    private _tokenService: TokenService) {
      this.createForm();
     }

  ngOnInit(): void {
  }

  createForm() {
    this.form = this._fb.group({
      inviteCode: ['', [Validators.required, createNotWhitespaceValidator(), Validators.maxLength(100), createIsServerErrorValidator(this.isServerError)] ],
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
    let inviteCodeControl = this.form.get('inviteCode');
    this._invitesService.join(this._tokenService.getJwtToken()!, inviteCodeControl?.value)
    .subscribe({
      next: result => {
        this.closeOuter.emit();
        this._router.navigate(['projects', result.projectId]);
      },
      error: error => {
        this.loading = false;
        this.validationMessages.inviteCode.notServerError = error?.error ?? JSON.stringify(error);
        this.isServerError.value = true;
        inviteCodeControl?.markAsTouched();
        inviteCodeControl?.markAsDirty();
        inviteCodeControl?.updateValueAndValidity();
        this.onValueChanged();
        inviteCodeControl?.valueChanges.pipe(first())
        .subscribe(() => 
        {
            this.isServerError.value = false;
            inviteCodeControl?.updateValueAndValidity();
        })
      }
    });
  }

}

import { CdkTextareaAutosize } from '@angular/cdk/text-field';
import { Component, EventEmitter, Inject, OnInit, Output, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { BanService } from 'src/app/services/ban.service';
import { AddBanModel } from 'src/app/shared/ban/add-ban.model';
import { BanAddPageModel } from 'src/app/shared/ban/ban-add-page.model';
import { ConnectionContainer } from 'src/app/shared/other/connection-container';
import { BannedModel } from 'src/app/shared/user/banned.model';
import { UserProfileReducedModel } from 'src/app/shared/user/user-profile-reduced.model';

@Component({
  selector: 'app-ban-add',
  templateUrl: './ban-add.component.html',
  styleUrls: ['./ban-add.component.scss']
})
export class BanAddComponent implements OnInit {
  @Output() succeeded : EventEmitter<BannedModel> = new EventEmitter<BannedModel>();
  user: UserProfileReducedModel;
  success: boolean = false;
  loading: boolean = false;
  errorMessage: string | null | undefined;
  @ViewChild('reasonAutosize') nameAutosize: CdkTextareaAutosize = undefined!;
  minDate: Date;
  form: FormGroup = null!;
  @ViewChild('bform') formDirective: any;

  formErrors : any = {
    'reason': ''
  };

  validationMessages : any = {
    'reason': {
      'maxlength':      'Reason max size is 400 characters'
    }
  };

  connectionContainer: ConnectionContainer = new ConnectionContainer();

  constructor(private _dialogRef: MatDialogRef<BanAddComponent>, @Inject(MAT_DIALOG_DATA) public data: BanAddPageModel, 
  private _fb: FormBuilder, private _banService: BanService) {
    this.user = data.user;
    this.connectionContainer = data.connectionContainer;
    this.minDate = new Date();
    this.minDate.setMinutes(this.minDate.getMinutes() + 1);
    this.createForm();
   }

  ngOnInit(): void {
  }

  createForm() {
    this.form = this._fb.group({
      reason: ['', [Validators.maxLength(400)] ],
      endDate: ['', Validators.required],
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
          if (messages)
          for (const key in control.errors) {
            if (control.errors.hasOwnProperty(key)) {
              this.formErrors[field] += messages[key] + ' ';
            }
          }
        }
      }
    }
  }

  public onSubmit(): void
  {
    if (!this.form.valid)
      return;
    const model: AddBanModel = 
    {
      hammer: this.form.get('reason')?.value,
      dateTo: this.form.get('endDate')?.value,
      userId: this.user.id
    };
    this.loading = true;
    this._banService.add(this.connectionContainer.id, model)
    .subscribe({
      next: result =>
      {
        this.loading = false;
        this.success = true;
        this.succeeded.emit(result);
      },
      error: error =>
      {
        this.loading = false;
        const message = `An error occured: ${error.status} - ${error.statusText || ''}\n${JSON.stringify(error.error)}`;
        this.errorMessage = message;
        console.error(message);
      }
    });
  }
}

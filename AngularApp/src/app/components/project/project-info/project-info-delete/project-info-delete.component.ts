import { Component, Inject, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { createIsEqualToValueValidator } from 'src/app/customvalidators';
import { ProjectService } from 'src/app/services/project.service';
import { TokenService } from 'src/app/services/token.service';
import { ProjectMiniModel } from 'src/app/shared/project/project-mini.model';
import { ProjectInfoComponent } from '../project-info.component';

@Component({
  selector: 'app-project-info-delete',
  templateUrl: './project-info-delete.component.html',
  styleUrls: ['./project-info-delete.component.scss']
})
export class ProjectInfoDeleteComponent implements OnInit {
  loading: boolean = false;
  success: boolean = false;
  errorMessage: string | null | undefined;
  private _projectId: number;
  projectName: string;
  form: FormGroup = null!;
  @ViewChild('dform') formDirective: any;

  formErrors : any = {
    'name': '',
  };

  validationMessages : any = {
    'name': {
      'matchesToValue': 'Wrong value'
    },
  };

  constructor(private _dialogRef: MatDialogRef<ProjectInfoComponent>, 
    @Inject(MAT_DIALOG_DATA) public data: ProjectMiniModel, private _router: Router,
    private _projectService: ProjectService,
    private _tokenService: TokenService,
    private _fb: FormBuilder) { 
    this._projectId = data.id;
    this.projectName = data.name;
    this.createForm();
    this._dialogRef.afterClosed()
    .subscribe(() => {
      if (this.success)
        this._router.navigate(['projects']);
    });
  }

  ngOnInit(): void {
  }

  createForm() {
    this.form = this._fb.group({
      name: ['', [createIsEqualToValueValidator(this.projectName)] ],
      description: ''
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

  onSubmit() : void {
    this.loading = true;
    if (this.form.valid)
    {
      this._projectService.delete(this._tokenService.getJwtToken()!, this._projectId).subscribe(
      {
        next: () => {
          this.success = true;
          this.loading = false;
        },
        error: error => {
          this.errorMessage = `${error.status} - ${error.statusText || ''}\n${JSON.stringify(error.error)}`;
          this.loading = false;
        }
      }
      );
    }
  }
}

import { CdkTextareaAutosize } from '@angular/cdk/text-field';
import { Component, EventEmitter, Input, NgZone, OnInit, Output, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { createNotWhitespaceValidator } from 'src/app/customvalidators';
import { ProjectService } from 'src/app/services/project.service';
import { TokenService } from 'src/app/services/token.service';
import { ProjectModel } from 'src/app/shared/project/project.model';
import { take } from 'rxjs';
import { UpdateProjectModel } from 'src/app/shared/project/update-project.model';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-project-info-edit',
  templateUrl: './project-info-edit.component.html',
  styleUrls: ['./project-info-edit.component.scss']
})
export class ProjectInfoEditComponent implements OnInit {
  @Input() project: ProjectModel = undefined!;
  form: FormGroup = null!;
  @ViewChild('uform') formDirective: any;
  @ViewChild('descriptionAutosize') descriptionAutosize: CdkTextareaAutosize = undefined!;
  model: UpdateProjectModel = {name: '', description: undefined};
  @Output() projectNameChange = new EventEmitter<string>();

  formErrors : any = {
    'name': '',
  };

  validationMessages : any = {
    'name': {
      'required':      'Name is required.',
      'notWhitespace':      'Name cannot be whitespace-only.',
      'maxlength':     'Name cannot be more than 150 characters long.',
    },
  };


  constructor(private _fb: FormBuilder,
    private _projectsService: ProjectService,
    private _tokenService: TokenService,
    private _ngZone: NgZone,
    private _snackBar: MatSnackBar) {
      this.createForm();
     }

  ngOnInit(): void {
    this.model.name = this.project.name;
    this.model.description = this.project.description;
    this.form.controls['name'].setValue(this.model.name);
    this.form.controls['description'].setValue(this.model.description);
  }

  createForm() {
    this.form = this._fb.group({
      name: ['', [Validators.required, createNotWhitespaceValidator(), Validators.maxLength(150)] ],
      description: ''
    });

    this.form.valueChanges
    .subscribe(data => this.onValueChanged(data));

    this.onValueChanged();
  }

  triggerResize() {
    this._ngZone.onStable.pipe(take(1)).subscribe(() => this.descriptionAutosize.resizeToFitContent(true));
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
    type ObjectKey = keyof typeof this.model;
    const callerNameAsKey = callerName as ObjectKey;
    if ((this.model[callerNameAsKey]) != event.target.value && this.form.valid)
    {
      this.model = this.form.value;
    this._projectsService.update(this._tokenService.getJwtToken()!, this.project.id, this.model!)
    .subscribe({
      next: () => {
        this._snackBar.open("Updated successfully", "Close", {
          duration: 1000,
          panelClass: "snackbar-orange"
        });
        this.project.name = this.model.name;
        this.project.description = this.model.description;
        if (callerName == 'name')
          this.projectNameChange.emit(this.project.name);
      },
      error: error => { 
        this._snackBar.open(`The project was not updated: ${error.status} - ${error.statusText || ''}\n${JSON.stringify(error.error)}`, "Close", {
          duration: 5000,
        });
      }
    });
  }
  }
}

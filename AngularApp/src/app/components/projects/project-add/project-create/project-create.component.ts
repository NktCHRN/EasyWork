import { CdkTextareaAutosize } from '@angular/cdk/text-field';
import { Component, EventEmitter, NgZone, OnInit, Output, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ProjectService } from 'src/app/services/project.service';
import { createNotWhitespaceValidator } from 'src/app/customvalidators';
import { take } from 'rxjs';
import { AddProjectModel } from 'src/app/shared/project/add-project.model';

@Component({
  selector: 'app-project-create',
  templateUrl: './project-create.component.html',
  styleUrls: ['./project-create.component.scss']
})
export class ProjectCreateComponent implements OnInit {
  loading: boolean = false;
  form: FormGroup = null!;
  @ViewChild('cform') formDirective: any;
  errorMessage: string | null | undefined
  @Output() closeOuter = new EventEmitter();
  @ViewChild('descriptionAutosize') descriptionAutosize: CdkTextareaAutosize = undefined!;

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
    private _router: Router,
    private _projectsService: ProjectService,
    private _ngZone: NgZone) {
      this.createForm();
     }

  ngOnInit(): void {
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
    // Wait for changes to be applied, then trigger textarea resize.
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

  onSubmit() {
    this.loading = true;
    let model: AddProjectModel = this.form.value;
    this._projectsService.add(model)
    .subscribe({
      next: result => {
        this.closeOuter.emit();
        this._router.navigate(['projects', result.id]);
      },
      error: error => {
        this.errorMessage = error?.message ?? JSON.stringify(error);
        this.loading = false;
      }
    });
  }
}

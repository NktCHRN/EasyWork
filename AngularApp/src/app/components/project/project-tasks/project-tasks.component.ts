import { Component, Inject, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Title } from '@angular/platform-browser';
import { createNumberOrUnlimitedValidator } from 'src/app/customvalidators';
import { ProjectService } from 'src/app/services/project.service';
import { TokenService } from 'src/app/services/token.service';
import { ProjectLimitsModel } from 'src/app/shared/project/limits/project-limits.model';
import { UserOnProjectRole } from 'src/app/shared/project/user-on-project/role/user-on-project-role';
import { UserOnProjectReducedModel } from 'src/app/shared/project/user-on-project/user-on-project-reduced.model';

@Component({
  selector: 'app-project-tasks',
  templateUrl: './project-tasks.component.html',
  styleUrls: ['./project-tasks.component.scss']
})
export class ProjectTasksComponent implements OnInit {
  projectId: number = undefined!;
  projectName: string = undefined!;
  me: UserOnProjectReducedModel = undefined!;
  userOnProjectRoles = UserOnProjectRole;
  limits: ProjectLimitsModel = undefined!;

  form: FormGroup = null!;
  @ViewChild('lform') formDirective: any;

  formErrors : any = {
    'toDo': '',
    'inProgress': '',
    'validate': ''
  };

  validationMessages : any = {
    'toDo': {
      'required':      'Max quantity is required.',
      'numberOrUnlimited': 'Max quantity must be a positive number, a zero or "unlimited"'
    },
    'inProgress': {
      'required':      'Max quantity is required.',
      'numberOrUnlimited': 'Max quantity must be a positive number, a zero or "unlimited"'
    },
    'validate': {
      'required':      'Max quantity is required.',
      'numberOrUnlimited': 'Max quantity must be a positive number, a zero or "unlimited"'
    }
  };

  constructor(private _titleService: Title, @Inject('projectName') private _websiteName: string, 
  private _fb: FormBuilder, private _snackBar: MatSnackBar, private _projectService: ProjectService,
  private _tokenService: TokenService) { 
    this.createForm();
  }

  ngOnInit(): void {
    this._titleService.setTitle(`${this.projectName} | Tasks - ${this._websiteName}`);
    const toDoControl = this.form.controls['toDo'];
    const inProgressControl = this.form.controls['inProgress'];
    const validateControl = this.form.controls['validate'];
    this._projectService.getLimits(this._tokenService.getJwtToken()!, this.projectId)
    .subscribe({
      next: result => 
      {
        this.limits = result;
        if (result.maxToDo)
          toDoControl.setValue(result.maxToDo);
        if (result.maxInProgress)
          inProgressControl.setValue(result.maxInProgress);
        if (result.maxValidate)
          validateControl.setValue(result.maxValidate);
      },
      error: error => 
      this._snackBar.open("The max quantities have not been loaded. Error: " + JSON.stringify(error), "Close", {duration: 5000})
    });
    if (this.me.role < this.userOnProjectRoles.Manager)
    {
      const controls = [toDoControl, inProgressControl, validateControl];
      controls.forEach(control => control.disable());
    }
  }

  public changeLimit(data: any): string
  {
    return data.toString().replace(/\s+$/g, '');
  }

  createForm() {
    const toDoControl = new FormControl({value: 'unlimited', disabled: false}, [Validators.required, createNumberOrUnlimitedValidator()]);
    const inProgressControl = new FormControl({value: 'unlimited', disabled: false}, [Validators.required, createNumberOrUnlimitedValidator()]);
    const validateControl = new FormControl({value: 'unlimited', disabled: false}, [Validators.required, createNumberOrUnlimitedValidator()]);
    this.form = this._fb.group({
      toDo: toDoControl,
      inProgress: inProgressControl,
      validate: validateControl
    });
    const controls = [toDoControl, inProgressControl, validateControl];
    controls.forEach(control => {
      control.valueChanges
      .subscribe(data => control.setValue(this.changeLimit(data), {emitEvent: false}));
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

  onSubmit(event: any) {
    let callerName:string = event.target.attributes.getNamedItem('ng-reflect-name').value;
    callerName = 'max' + callerName.charAt(0).toUpperCase() + callerName.slice(1);
    type ObjectKey = keyof typeof this.limits;
    const callerNameAsKey = callerName as ObjectKey;
    const value = event.target.value == "unlimited" ? undefined : event.target.value;
    if (this.limits[callerNameAsKey] != value && this.form.valid)
    {
      const toDo = parseInt(this.form.get('toDo')?.value);
      const inProgress = parseInt(this.form.get('inProgress')?.value);
      const validate = parseInt(this.form.get('validate')?.value);
      const newLimits:ProjectLimitsModel = 
      {
        maxToDo: isNaN(toDo) ? undefined : toDo,
        maxInProgress: isNaN(inProgress) ? undefined : inProgress,
        maxValidate: isNaN(validate) ? undefined : validate
      };
      this._projectService.updateLimits(this._tokenService.getJwtToken()!, this.projectId, newLimits)
      .subscribe({
        next: () => {
          this.limits = newLimits;
          this._snackBar.open("The max quantities were updated successfully", "Close", {
            duration: 1000,
            panelClass: "snackbar-orange"
          });
        },
        error: error => { 
          this._snackBar.open(`The max quantities were not updated: ${error.status} - ${error.statusText || ''}\n${JSON.stringify(error.error)}`, "Close", {
            duration: 5000,
          });
        }
      });
    }
  }
}

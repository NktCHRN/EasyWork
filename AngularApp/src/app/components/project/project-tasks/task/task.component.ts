import { CdkTextareaAutosize } from '@angular/cdk/text-field';
import { Component, Inject, NgZone, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { createNotWhitespaceValidator } from 'src/app/customvalidators';
import { TaskService } from 'src/app/services/task.service';
import { TokenService } from 'src/app/services/token.service';
import { SavedIconState } from 'src/app/shared/task/save/saved-icon-state';
import { TaskModel } from 'src/app/shared/task/task.model';

@Component({
  selector: 'app-task',
  templateUrl: './task.component.html',
  styleUrls: ['./task.component.scss']
})
export class TaskComponent implements OnInit {
  private _taskId: number;
  task: TaskModel = undefined!;
  iconStates = SavedIconState;
  savedIconState: SavedIconState | null = null;
  @ViewChild('nameAutosize') nameAutosize: CdkTextareaAutosize = undefined!;
  @ViewChild('descriptionAutosize') descriptionAutosize: CdkTextareaAutosize = undefined!;
  editName: boolean = false;
  savedIconColors = {
    [SavedIconState.Fail] : "#D84315",
    [SavedIconState.Loading] : "gray",
    [SavedIconState.Success] : "seagreen"
  };
  form: FormGroup = null!;
  @ViewChild('tform') formDirective: any;

  formErrors : any = {
    'name': '',
  };

  validationMessages : any = {
    'name': {
      'required':      'Name is required.',
      'notWhitespace':      'Name cannot be whitespace-only.'
    },
  };

  constructor(private _dialogRef: MatDialogRef<TaskComponent>, @Inject(MAT_DIALOG_DATA) public data: number, 
  private _taskService: TaskService, private _tokenService: TokenService, private _snackBar: MatSnackBar, 
  private _fb: FormBuilder) { 
    this._taskId = data;
    this.createForm();
  }

  toggleEditName(): void {
    this.editName = !this.editName;
  }

  createForm() {
    this.form = this._fb.group({
      name: ['', [Validators.required, createNotWhitespaceValidator()] ],
      description: '',
      deadline: '',
      endDate: ''
    });

    this.form.valueChanges
    .subscribe(data => this.onValueChanged(data));

    this.onValueChanged();
  }

  ngOnInit(): void {
    this._taskService.getById(this._tokenService.getJwtToken()!, this._taskId)
    .subscribe({
      next: result => 
      {
        this.task = result;
        this.form.controls['name'].setValue(this.task.name);
        this.form.controls['description'].setValue(this.task.description);
        this.form.controls['deadline'].setValue(this.task.deadline);
        this.form.controls['endDate'].setValue(this.task.endDate);
      },
      error: error => {
        this._snackBar.open("Task has not been loaded. Error: " + JSON.stringify(error), "Close", {duration: 5000});
      }
    });
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
    console.log(this.form.value);
  }
}

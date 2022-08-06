import { Component, Input, NgZone, OnInit, Output, ViewChild } from '@angular/core';
import { ProjectService } from 'src/app/services/project.service';
import { TaskStatus } from 'src/app/shared/task/status/task-status';
import { TaskReducedModel } from 'src/app/shared/task/task-reduced.model';
import { EventEmitter } from '@angular/core';
import { CdkTextareaAutosize } from '@angular/cdk/text-field';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { createNotWhitespaceValidator } from 'src/app/customvalidators';
import { take } from 'rxjs';
import { AddTaskModel } from 'src/app/shared/task/add-task.model';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ConnectionContainer } from 'src/app/shared/other/connection-container';

@Component({
  selector: 'app-task-add',
  templateUrl: './task-add.component.html',
  styleUrls: ['./task-add.component.scss']
})
export class TaskAddComponent implements OnInit {
  @Input() taskStatus: TaskStatus = TaskStatus.ToDo;
  @Input() projectId: number = undefined!;
  isOpened: boolean = false;
  @Output() added: EventEmitter<TaskReducedModel> = new EventEmitter<TaskReducedModel>();
  loading: boolean = false;
  @ViewChild('nameAutosize') nameAutosize: CdkTextareaAutosize = undefined!;
  form: FormGroup = null!;
  @ViewChild('aform') formDirective: any;

  formErrors : any = {
    'name': '',
  };

  validationMessages : any = {
    'name': {
      'required':      'Name is required.',
      'notWhitespace':      'Name cannot be whitespace-only.'
    },
  };

  @Input() projectsConnectionContainer: ConnectionContainer = new ConnectionContainer();

  constructor(private _projectService: ProjectService,
    private _fb: FormBuilder, private _ngZone: NgZone, private _snackBar: MatSnackBar) { 
      this.createForm();
    }

  ngOnInit(): void {
  }

  toggle(): void {
    this.isOpened = !this.isOpened;
  }

  createForm() {
    this.form = this._fb.group({
      name: ['', [Validators.required, createNotWhitespaceValidator()] ],
      description: ''
    });

    this.form.valueChanges
    .subscribe(data => this.onValueChanged(data));

    this.onValueChanged();
  }

  triggerResize() {
    this._ngZone.onStable.pipe(take(1)).subscribe(() => this.nameAutosize.resizeToFitContent(true));
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
    if (this.form.valid)
    {
      this.loading = true;
      const model: AddTaskModel = {
        name: this.form.get('name')?.value,
        status: this.taskStatus
      };
      this._projectService.addTask(this.projectsConnectionContainer.id, this.projectId, model)
      .subscribe({
        next: result => {
          this.loading = false;
          const reducedResult: TaskReducedModel = {
            filesCount: 0,
            messagesCount: 0,
            executors: [],
            ...result
          };
          this.form.reset();
          this.isOpened = false;
          this.added.emit(reducedResult);
        },
        error: error => {
          this.loading = false;
          this._snackBar.open(`Task has not been added: ${error.status} - ${error.statusText || ''}\n${JSON.stringify(error.error)}`, "Close", {
            duration: 5000,
          });
        }
      });
    }
  }
}

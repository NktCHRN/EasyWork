import { Component, Inject, OnInit, QueryList, ViewChild, ViewChildren } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Title } from '@angular/platform-browser';
import { createNumberOrUnlimitedValidator } from 'src/app/customvalidators';
import { ProjectService } from 'src/app/services/project.service';
import { TaskService } from 'src/app/services/task.service';
import { TokenService } from 'src/app/services/token.service';
import { ProjectLimitsModel } from 'src/app/shared/project/limits/project-limits.model';
import { TasksCountModel } from 'src/app/shared/project/tasks/tasks-count.model';
import { TasksModel } from 'src/app/shared/project/tasks/tasks.model';
import { UserOnProjectRole } from 'src/app/shared/project/user-on-project/role/user-on-project-role';
import { UserOnProjectReducedModel } from 'src/app/shared/project/user-on-project/user-on-project-reduced.model';
import { TagModel } from 'src/app/shared/tag/tag.model';
import { TaskStatus } from 'src/app/shared/task/status/task-status';
import { TaskStatusChangeModel } from 'src/app/shared/task/status/task-status-change.model';
import { TaskStatusWithDescriptionModel } from 'src/app/shared/task/status/task-status-with-description.model';
import { TaskReducedWithStatusModel } from 'src/app/shared/task/task-reduced-with-status.model';
import { TaskReducedModel } from 'src/app/shared/task/task-reduced.model';
import { ProjectTagDeleteComponent } from './project-tag-delete/project-tag-delete.component';
import { TaskReducedComponent } from './task-reduced/task-reduced.component';

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
  tasks: TasksModel = {
    toDo: undefined!,
    inProgress: undefined!,
    validate: undefined!,
    done: undefined!
  };
  tasksCount: TasksCountModel = {
    toDo: undefined!,
    inProgress: undefined!,
    validate: undefined!,
    done: undefined!
  }
  loadError: boolean = false;
  tags: TagModel[] = [
    {
      id: 0,
      name: "All"
    }
  ];
  selectedTag: TagModel;
  taskStatuses = TaskStatus;
  selectedStatus: TaskStatus = TaskStatus.ToDo;
  readonly statusesWithDescription: TaskStatusWithDescriptionModel[];

  form: FormGroup = null!;
  @ViewChild('lform') formDirective: any;
  @ViewChildren(TaskReducedComponent) viewTasks: QueryList<TaskReducedComponent> = undefined!;

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
  private _tokenService: TokenService, private _dialog: MatDialog, private _taskService: TaskService) { 
    this.statusesWithDescription = this._taskService.getStatusesWithDescriptions(false);
    this.createForm();
    this.selectedTag = this.tags[0];
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
      this._snackBar.open("Max quantities have not been loaded. Error: " + JSON.stringify(error), "Close", {duration: 5000})
    });
    if (this.me.role < this.userOnProjectRoles.Manager)
    {
      const controls = [toDoControl, inProgressControl, validateControl];
      controls.forEach(control => control.disable());
    }
    this._projectService.getTasks(this._tokenService.getJwtToken()!, this.projectId, null)
    .subscribe({
      next: result => 
      {
        this.tasks = result;
        for (let key in result)
          this.tasksCount[key as keyof TasksCountModel] = this.tasks[key as keyof TasksModel].length;
      },
      error: error => 
      {
        this.loadError = true;
        this._snackBar.open("Tasks have not been loaded. Error: " + JSON.stringify(error), "Close", {duration: 5000});
      }
    });
    this._projectService.getTags(this._tokenService.getJwtToken()!, this.projectId)
    .subscribe({
      next: result => 
      {
        this.tags = this.tags.concat(result);
      },
      error: error => 
      {
        this.loadError = true;
        this._snackBar.open("Tags have not been loaded. Error: " + JSON.stringify(error), "Close", {duration: 5000});
      }
    });
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
          this._snackBar.open("Max quantities were updated successfully", "Close", {
            duration: 1000,
            panelClass: "snackbar-orange"
          });
        },
        error: error => { 
          this._snackBar.open(`Max quantities were not updated: ${error.status} - ${error.statusText || ''}\n${JSON.stringify(error.error)}`, "Close", {
            duration: 5000,
          });
        }
      });
    }
  }

  changeSelectedTag(tag: TagModel): void {
    const id = tag.id == 0 ? null : tag.id;
    this._projectService.getTasks(this._tokenService.getJwtToken()!, this.projectId, id)
    .subscribe({
      next: result => 
      {
        this.tasks = result;
        this.selectedTag = tag;
      },
      error: error => 
      {
        this.loadError = true;
        this._snackBar.open("Tasks with chosen tag have not been loaded. Error: " + JSON.stringify(error), "Close", {duration: 5000});
      }
    });
  }

  openRemoveTagDialog(tag: TagModel): void {
    if (tag.id == 0)
    return;
    const dialogRef = this._dialog.open(ProjectTagDeleteComponent, {
      panelClass: "dialog-responsive",
      data: {
        tag: tag,
        projectId: this.projectId
      }
    });
    dialogRef.afterClosed()
    .subscribe(() => {
      if (dialogRef.componentInstance.success)
      {
          this.tags.splice(this.tags.indexOf(tag), 1);
          if (this.selectedTag == tag)
            this.changeSelectedTag(this.tags[0]);
      };
    });
  }

  convertStatusToString(status: TaskStatus): string {
    let modifiedStatus: string
    if (status == TaskStatus.Complete)
      modifiedStatus = 'done';
    else
      modifiedStatus = status.charAt(0).toLowerCase() + status.slice(1);
    return modifiedStatus;
  }

  getTasksColumnByStatus(status: string): TaskReducedModel[] {
    type ObjectKey = keyof typeof this.tasks;
    return this.tasks[status as ObjectKey]
  }

  changeCountByStatus(status: string, toAdd: number) {
    type ObjectKey = keyof TasksCountModel;
    this.tasksCount[status as ObjectKey] += toAdd;
  }

  addTask(task: TaskReducedModel, where: TaskStatus) {
    const convertedWhere: string = this.convertStatusToString(where);
    this.getTasksColumnByStatus(convertedWhere).push(task);
    this.changeCountByStatus(convertedWhere, 1);
  }

  onAddedWithTagError(where: TaskStatus) {
    this.changeCountByStatus(this.convertStatusToString(where), 1);
  }

  onTaskStatusUpdate(event: TaskStatusChangeModel): void {
    if (event.old == TaskStatus.Archived)
      return;
    const convertedOld: string = this.convertStatusToString(event.old);
    const oldTasks = this.getTasksColumnByStatus(convertedOld);
    const found = oldTasks?.find(t => t.id == event.id);
    if (found)
    {
      const oldIndex = oldTasks.indexOf(found);
      if (oldIndex != -1)
      {
        this.changeCountByStatus(convertedOld, -1);
        oldTasks.splice(oldIndex, 1);
      }
      else if (this.selectedTag.id != 0)
        this.changeCountByStatus(convertedOld, -1);
      if (event.new != TaskStatus.Archived)
        this.addExistingTask(found, event.new);
      this.subscribeToTask(found.id);
    }
  }

  private addExistingTask(task: TaskReducedModel, where: TaskStatus): void {
    const convertedWhere: string = this.convertStatusToString(where);
    const newTasks = this.getTasksColumnByStatus(convertedWhere);
    this.changeCountByStatus(convertedWhere, 1);
    newTasks.splice(this._taskService.getInsertAtIndexByTaskId(task.id, newTasks), 0, task);
  }

  private subscribeToTask(taskId: number): void {
    const foundViewTask = this.viewTasks.find(t => t.model.id == taskId);
    foundViewTask?.updatedStatus.subscribe(m => this.onTaskStatusUpdate(m));
    foundViewTask?.movedFromArchived.subscribe(m => this.onAddFromArchive(m));
  }

  onAddFromArchive(task: TaskReducedWithStatusModel): void
  {
    this.addExistingTask(task, task.status);
    this.subscribeToTask(task.id);
  }
}

import { Component, EventEmitter, Inject, OnInit, Output, ViewChild } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { filter, ReplaySubject, tap, takeUntil, debounceTime, Subject, map, switchMap } from 'rxjs';
import { ProjectRoleService } from 'src/app/services/project-role.service';
import { ProjectService } from 'src/app/services/project.service';
import { UserService } from 'src/app/services/user.service';
import { ConnectionContainer } from 'src/app/shared/other/connection-container';
import { AddUserOnProjectModel } from 'src/app/shared/project/user-on-project/add/add-user-on-project.model';
import { ProjectAddUserPageModel } from 'src/app/shared/project/user-on-project/add/project-add-user-page.model';
import { RoleWithDescription } from 'src/app/shared/project/user-on-project/role/role-with-description.model';
import { UserOnProjectRole } from 'src/app/shared/project/user-on-project/role/user-on-project-role';
import { UserOnProjectExtendedModel } from 'src/app/shared/project/user-on-project/user-on-project-extended.model';
import { UserOnProjectModel } from 'src/app/shared/project/user-on-project/user-on-project.model';
import { UserProfileReducedModel } from 'src/app/shared/user/user-profile-reduced.model';

@Component({
  selector: 'app-project-user-add',
  templateUrl: './project-user-add.component.html',
  styleUrls: ['./project-user-add.component.scss']
})
export class ProjectUserAddComponent implements OnInit {
  loading: boolean = false;
  errorMessage: string | null | undefined;
  private _projectId: number;
  projectName: string;
  myRole: UserOnProjectRole;
  roles: RoleWithDescription[] = [];
  form: FormGroup = null!;
  @ViewChild('aform') formDirective: any;
  selectedRole: RoleWithDescription;
  users: ReplaySubject<UserProfileReducedModel[]> = new ReplaySubject<UserProfileReducedModel[]>(1);
  newUser: UserOnProjectExtendedModel | null | undefined;
  public searching = false;
  public usersFilteringCtrl: FormControl = new FormControl();
  private _projectUserIds: number[];
  @Output() succeeded: EventEmitter<UserOnProjectExtendedModel> = new EventEmitter<UserOnProjectExtendedModel>();

  formErrors : any = {
    'user': ''
  };

  validationMessages : any = {
    'user': {
      'required':      'User is required.'
    }
  };

  connectionContainer: ConnectionContainer;

  constructor(private _dialogRef: MatDialogRef<ProjectUserAddComponent>, 
    @Inject(MAT_DIALOG_DATA) public data: ProjectAddUserPageModel,
    private _projectService: ProjectService, public roleService: ProjectRoleService,
    private _fb: FormBuilder, private _userService: UserService) { 
      this._projectId = data.project.id;
      this.projectName = data.project.name;
      this._projectUserIds = data.project.users.map(u => u.user.id);
      this.myRole = data.myRole;
      this.connectionContainer = data.connectionContainer;
      this.roles = this.roleService.getRolesWithDescription(this.myRole);
      this.selectedRole = this.roles[0];
      this.createForm();
  }

  protected _onDestroy = new Subject<void>();

  createForm() {
    const currentRole = this.roleService.roleToString(this.roles[0].role);
    const roleControl = new FormControl(currentRole);
    this.form = this._fb.group({
      user: [undefined, [Validators.required]],
      role: roleControl
    });

    roleControl.valueChanges
    .subscribe(value => this.changeSelectedRole(value));
    this.changeSelectedRole(currentRole);

    this.usersFilteringCtrl.valueChanges
    .pipe(
      filter(search => search.length >= 3),
      tap(() => this.searching = true),
      takeUntil(this._onDestroy),
      debounceTime(200),
      switchMap(search => {
        return this._userService.get(search)
        .pipe(map(result => result.filter(u => !this._projectUserIds.includes(u.id))));
      }),
      takeUntil(this._onDestroy)
    )
    .subscribe(
      { 
        next: result => {
          this.searching = false;
          this.users.next(result);
        },
        error: error => {
          this.searching = false;
      }});

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

  ngOnInit(): void {
    this.connectionContainer.connection.on("AddedUser", (model: UserOnProjectModel) => {
      if (model.projectId == this._projectId && this._projectUserIds)
      {
        const found = this._projectUserIds.findIndex(u => u == model.userId);
        if (found == -1)
          this._projectUserIds.push(model.userId);
      }
    });
    this.connectionContainer.connection.on("DeletedUser", (projectId: number, userId: number) => {
      if (projectId == this._projectId && this._projectUserIds){
        const found = this._projectUserIds.findIndex(u => u == userId);
        if (found != -1)
          this._projectUserIds.splice(found, 1);
      }
    });
  }

  onSubmit(): void {
    this.loading = true;
    const user: UserProfileReducedModel = this.form.get('user')?.value;
    let model: AddUserOnProjectModel = {
      role: this.form.get('role')?.value,
      userId: user.id
    };
    this._projectService.addUser(this.connectionContainer.id, this._projectId, model).subscribe(
    {
      next: result => {
        const role = this.roleService.roleToEnum(result.role);
        this.newUser = {
          role: role,
          tasksNotDone: 0,
          tasksDone: 0,
          isKickable: this.roleService.isKickable(this.myRole, role),
          user: {
            id: result.userId,
            fullName: user.fullName,
            avatarURL: user.avatarURL
          }
        };
        this.loading = false;
        this.succeeded.emit(this.newUser);
      },
      error: error => {
        this.errorMessage = `${error.status} - ${error.statusText || ''}\n${JSON.stringify(error.error)}`;
        this.loading = false;
      }
    }
    );
  }

  changeSelectedRole($event: string): void {
    this.selectedRole = this.roles.find(r => UserOnProjectRole[r.role] == $event)!;
  }

  ngOnDestroy() {
    this._onDestroy.next();
    this._onDestroy.complete();
  }

}

import { Component, Inject, OnInit, ViewChild } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatTable } from '@angular/material/table';
import { Title } from '@angular/platform-browser';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable } from 'rxjs';
import { ProjectRoleService } from 'src/app/services/project-role.service';
import { ProjectService } from 'src/app/services/project.service';
import { UserService } from 'src/app/services/user.service';
import { ConnectionContainer } from 'src/app/shared/other/connection-container';
import { UserOnProjectRole } from 'src/app/shared/project/user-on-project/role/user-on-project-role';
import { UserOnProjectExtendedModel } from 'src/app/shared/project/user-on-project/user-on-project-extended.model';
import { UserOnProjectReducedModel } from 'src/app/shared/project/user-on-project/user-on-project-reduced.model';
import { UserOnProjectModel } from 'src/app/shared/project/user-on-project/user-on-project.model';
import { StatsChangeModel } from 'src/app/shared/task/executor/stats-change.model';
import { ErrorDialogComponent } from '../../error-dialog/error-dialog.component';
import { ProjectKickComponent } from './project-kick/project-kick.component';
import { ProjectLeaveComponent } from './project-leave/project-leave.component';
import { ProjectUserAddComponent } from './project-user-add/project-user-add.component';
import { ProjectUserEditComponent } from './project-user-edit/project-user-edit.component';

@Component({
  selector: 'app-project-participants',
  templateUrl: './project-participants.component.html',
  styleUrls: ['./project-participants.component.scss']
})
export class ProjectParticipantsComponent implements OnInit {
  projectId: number = undefined!;
  projectName: string = undefined!;
  me: UserOnProjectReducedModel = undefined!;
  userOnProjectRoles = UserOnProjectRole;
  loading: boolean = true;
  users: UserOnProjectExtendedModel[] = undefined!;
  displayedColumns: string[] = ['name', 'role', 'tasksDone', 'tasksNotDone'];
  private _isSingleOwner: BehaviorSubject<boolean | null | undefined> = new BehaviorSubject<boolean | null | undefined>(undefined);
  public isSingleOwner: Observable<boolean | null | undefined> = this._isSingleOwner.asObservable();
  @ViewChild(MatTable) table: MatTable<UserOnProjectExtendedModel> = undefined!;

  connectionContainer: ConnectionContainer = new ConnectionContainer();

  constructor(private _titleService: Title, @Inject('projectName') private _websiteName: string, private _dialog: MatDialog, 
  private _projectService: ProjectService, private _router: Router, private _userService: UserService,
  public roleService: ProjectRoleService) { }

  ngOnInit(): void {
    this._titleService.setTitle(`${this.projectName} | Participants - ${this._websiteName}`);
    if (this.me.role >= this.userOnProjectRoles.Manager)
      this.displayedColumns.push('actions');
    this._projectService.getUsers(this.projectId, this.me.role)
    .subscribe({
      next: result => 
      {this.loading = false; this.users = result; this._isSingleOwner.next(this._projectService.isSingleOwner(this.users, this.me)); },
      error: error => 
      {
        this.loading = false;
        if (error.status == 404 || error.status == 400)
          this._router.navigate(["**"], { skipLocationChange: true });
        else
          this._dialog.open(ErrorDialogComponent, {
            panelClass: "dialog-responsive",
            data: JSON.stringify(error)
          });
      }
    });
    this.connectionContainer.connection.on("UpdatedUser", (model: UserOnProjectModel) => {
      if (model.projectId == this.projectId && this.users)
      {
        if (model.userId == this.me.userId)
        {
          this.me.role = this.roleService.roleToEnum(model.role);
          this.users.forEach(u => {
            u.isKickable = this.roleService.isKickable(this.me.role, u.role);
          });
          const colName = "actions";
          const foundIndex = this.displayedColumns.findIndex(c => c == colName);
          if (this.me.role >= this.userOnProjectRoles.Manager && foundIndex == -1)
            this.displayedColumns.push(colName);
          else if (this.me.role < this.userOnProjectRoles.Manager && foundIndex != -1)
            this.displayedColumns.splice(foundIndex, 1);
        }
        const found = this.users.find(u => model.userId == u.user.id)
        if (found)
          found.role = this.roleService.roleToEnum(model.role);
        this._isSingleOwner.next(this._projectService.isSingleOwner(this.users, this.me));
      }
    });
    this.connectionContainer.connection.on("TasksDoneChanged", (model: StatsChangeModel) => {
      if (model.projectId == this.projectId && this.users)
      {
        const found = this.users.find(u => model.userId == u.user.id)
        if (found)
          found.tasksDone += model.value;
      }
    });
    this.connectionContainer.connection.on("TasksNotDoneChanged", (model: StatsChangeModel) => {
      if (model.projectId == this.projectId && this.users)
      {
        const found = this.users.find(u => model.userId == u.user.id)
        if (found)
          found.tasksNotDone += model.value;
      }
    });
    this.connectionContainer.connection.on("AddedUser", (model: UserOnProjectModel) => {
      if (model.projectId == this.projectId && this.users)
      {
        const role = this.roleService.roleToEnum(model.role);
        this._userService.getById(model.userId)
        .subscribe({
          next: result => {
            this.AddUser({
              role: role,
              tasksNotDone: 0,
              tasksDone: 0,
              isKickable: this.roleService.isKickable(this.me.role, role),
              user: {
                id: model.userId,
                fullName: this._userService.getFullName(result.firstName, result.lastName),
                avatarURL: result.avatarURL
              }
            });
          },
          error: error => console.log(error)
        });
      }
    });
    this.connectionContainer.connection.on("DeletedUser", (projectId: number, userId: number) => {
      if (projectId == this.projectId && this.users)
        this.deleteUserById(userId);
    });
  }

  openLeaveDialog(): void {
    this._dialog.open(ProjectLeaveComponent, {
      panelClass: "dialog-responsive",
      data: {
        id: this.projectId,
        name: this.projectName
      }
    });
  }

  openKickDialog(id: number): void {
    const user = this.users.find(u => u.user.id == id);
    const dialogRef = this._dialog.open(ProjectKickComponent, {
      panelClass: "dialog-responsive",
      data: {
        project: {
          id: this.projectId,
          name: this.projectName
        },
        toKick: {
          id: user?.user.id,
          fullName: user?.user.fullName
        },
        connectionContainer: this.connectionContainer
      }
    });
    dialogRef.componentInstance.succeeded
    .subscribe(() => {
        this.deleteUser(user!);
    });
  }

  private deleteUser(user: UserOnProjectExtendedModel): void
  {
    this.deleteUserByIndex(this.users.indexOf(user));
  }

  private deleteUserById(id: number): void
  {
    const foundIndex = this.users.findIndex(u => u.user.id == id);
    if (foundIndex != -1)
      this.deleteUserByIndex(foundIndex);
  }

  private deleteUserByIndex(index: number): void
  {
    this.users.splice(index, 1);
    this.table.renderRows();
    this._isSingleOwner.next(this._projectService.isSingleOwner(this.users, this.me));
  }

  openEditDialog(id: number): void {
    const user = this.users.find(u => u.user.id == id);
    const dialogRef = this._dialog.open(ProjectUserEditComponent, {
      panelClass: "dialog-responsive",
      data: {
        project: {
          id: this.projectId,
          name: this.projectName
        },
        myRole: this.me.role,
        user: user,
        connectionContainer: this.connectionContainer
      }
    });
    dialogRef.componentInstance.succeeded
    .subscribe(() => {
        this._isSingleOwner.next(this._projectService.isSingleOwner(this.users, this.me));
    });
  }

  openAddDialog(): void {
    const dialogRef = this._dialog.open(ProjectUserAddComponent, {
      panelClass: "dialog-responsive",
      data: {
        project: {
          id: this.projectId,
          name: this.projectName,
          users: this.users
        },
        myRole: this.me.role,
        connectionContainer: this.connectionContainer
      }
    });
    dialogRef.componentInstance.succeeded
    .subscribe(user => {
        this.AddUser(user);
    });
  }

  private AddUser(user: UserOnProjectExtendedModel): void
  {
    let lastIndex = this.findLastWithRoleIndex(user.role);
    let index: number;
    if (lastIndex == -1)
    {
      if (user.role == UserOnProjectRole.Manager)
      {
        lastIndex = this.findLastWithRoleIndex(UserOnProjectRole.Owner);
        if (lastIndex == -1)
          index = this.users.length;
        else
          index = this.calculateReversedIndex(lastIndex);
      }
      else
        index = this.users.length;
    }
    else
       index = this.calculateReversedIndex(lastIndex);
    this.users.splice(index, 0, user);
    this.table.renderRows();
    this._isSingleOwner.next(this._projectService.isSingleOwner(this.users, this.me));
  }

  private findLastWithRoleIndex(role: UserOnProjectRole): number
  {
    return this.users.slice().reverse().findIndex(u => u.role == role)
  }

  private calculateReversedIndex(index: number): number {
    return this.users.length - index;
  }
}

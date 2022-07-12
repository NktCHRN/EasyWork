import { Component, Inject, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Title } from '@angular/platform-browser';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable } from 'rxjs';
import { ProjectRoleService } from 'src/app/services/project-role.service';
import { ProjectService } from 'src/app/services/project.service';
import { TokenService } from 'src/app/services/token.service';
import { UserOnProjectRole } from 'src/app/shared/project/role/user-on-project-role';
import { UserOnProjectExtendedModel } from 'src/app/shared/project/user-on-project/user-on-project-extended.model';
import { UserOnProjectReducedModel } from 'src/app/shared/project/user-on-project/user-on-project-reduced.model';
import { ErrorDialogComponent } from '../../error-dialog/error-dialog.component';
import { ProjectLeaveComponent } from './project-leave/project-leave.component';

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

  constructor(private _titleService: Title, @Inject('projectName') private _websiteName: string, private _dialog: MatDialog, 
  private _projectService: ProjectService, private _tokenService: TokenService, private _router: Router, 
  public roleService: ProjectRoleService) { }

  ngOnInit(): void {
    this._titleService.setTitle(`${this.projectName} | Participants - ${this._websiteName}`);
    if (this.me.role >= this.userOnProjectRoles.Manager)
      this.displayedColumns.push('actions');
    this._projectService.getUsers(this._tokenService.getJwtToken()!, this.projectId, this.me.role)
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
}

import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { ActivatedRoute } from '@angular/router';
import { ProjectService } from 'src/app/services/project.service';
import { TokenService } from 'src/app/services/token.service';
import { UserOnProjectRole } from 'src/app/shared/project/role/user-on-project-role';
import { UserOnProjectReducedModel } from 'src/app/shared/project/user-on-project/user-on-project-reduced.model';
import { ErrorDialogComponent } from '../error-dialog/error-dialog.component';

@Component({
  selector: 'app-project',
  templateUrl: './project.component.html',
  styleUrls: ['./project.component.scss']
})
export class ProjectComponent implements OnInit {
  projectId: number = undefined!;
  projectName: string = undefined!;
  me: UserOnProjectReducedModel = undefined!;
  userOnProjectRoles = UserOnProjectRole;
  
  constructor(private _route: ActivatedRoute, private _projectService: ProjectService, private _tokenService: TokenService, private _dialog: MatDialog) { }

  ngOnInit(): void {
    this._route.paramMap.subscribe(params => {
      this.projectId = parseInt(params.get('id')!);
      this._projectService.getReducedById(this._tokenService.getJwtToken()!, this.projectId)
      .subscribe({
        next: result => this.projectName = result.name,
        error: () => this.projectName = `Project ${this.projectId}`
      });
      this._projectService.getMeAsProjectUser(this._tokenService.getJwtToken()!, this.projectId)
      .subscribe({
        next: result => this.me = result,
        error: error => {
          this._dialog.open(ErrorDialogComponent, {
            panelClass: "dialog-responsive",
            data: JSON.stringify(error) + '\nPlease, reload the page'
          });
        }
      });
    });
  }

  onOutletLoaded(component: any): void {
    component.projectId = this.projectId;
    component.projectName = this.projectName;
    component.me = this.me;
    component.userOnProjectRoles = this.userOnProjectRoles;
    component.projectNameChange?.subscribe((result: string) => this.onProjectNameChange(result));
  } 

  onProjectNameChange(name: string): void 
  {
    this.projectName = name;
  }
}

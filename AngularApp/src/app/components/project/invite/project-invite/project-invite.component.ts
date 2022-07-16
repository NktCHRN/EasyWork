import { Component, Inject, Input, OnInit } from '@angular/core';
import { ProjectService } from 'src/app/services/project.service';
import { TokenService } from 'src/app/services/token.service';
import { InviteCodeModel } from 'src/app/shared/project/invite/invite-code.model';
import { UserOnProjectRole } from 'src/app/shared/project/user-on-project/role/user-on-project-role';
import { UserOnProjectReducedModel } from 'src/app/shared/project/user-on-project/user-on-project-reduced.model';
import {Clipboard} from "@angular/cdk/clipboard"
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { ProjectInviteRegenerateComponent } from '../project-invite-regenerate/project-invite-regenerate.component';

@Component({
  selector: 'app-project-invite',
  templateUrl: './project-invite.component.html',
  styleUrls: ['./project-invite.component.scss']
})
export class ProjectInviteComponent implements OnInit {
  @Input() projectId: number = undefined!;
  @Input() me: UserOnProjectReducedModel = undefined!;
  userOnProjectRoles = UserOnProjectRole;
  errorMessage: string | null | undefined;
  successCopy: boolean = false;
  successRegeneration: boolean = false;
  inviteCode: InviteCodeModel = undefined!;

  constructor(private _tokenService: TokenService, private _projectService: ProjectService, private _clipboard: Clipboard,
    private _snackBar: MatSnackBar, private _dialog: MatDialog, @Inject('appURL') private _appURL: string) { }

  ngOnInit(): void {
    this._projectService.getInviteCode(this._tokenService.getJwtToken()!, this.projectId)
    .subscribe({
      next: result => this.inviteCode = result,
      error: error => this.errorMessage = `${error.status} - ${error.statusText || ''}\n${JSON.stringify(error.error)}`
    });
  }

  copy(text: string): void {
      this._clipboard.copy(text);
      this.successCopy = true;
      setTimeout(() => this.successCopy = false, 5000);
  }

  toggleStatus(): void {
    this._projectService.changeInviteCodeStatus(this._tokenService.getJwtToken()!, this.projectId, {
      isActive: !this.inviteCode.isInviteCodeActive
    })
    .subscribe({
      next: () => this.inviteCode.isInviteCodeActive = !this.inviteCode.isInviteCodeActive,
      error: error => 
      this._snackBar.open(`The invite code status was not updated: ${error.status} - ${error.statusText || ''}\n${JSON.stringify(error.error)}`, 
      "Close", 
      {
        duration: 5000,
      }),
    });
  }

  openRegenerateDialog() : void
  {
    let dialogRef = this._dialog.open(ProjectInviteRegenerateComponent, {
      panelClass: "mini-dialog-responsive",
      data: this.projectId
    });
    dialogRef.componentInstance.inviteCodeChange.subscribe(result => 
      {
        this.inviteCode.inviteCode = result;
        this.successRegeneration = true;
        setTimeout(() => this.successRegeneration = false, 5000);
      }
    );
  }

  public get link(): string {
    return this._appURL + 'invite/' + this.inviteCode.inviteCode;
  }
}

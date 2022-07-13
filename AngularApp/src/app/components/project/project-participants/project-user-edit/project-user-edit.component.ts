import { Component, Inject, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormControl, FormGroup } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { ProjectRoleService } from 'src/app/services/project-role.service';
import { ProjectService } from 'src/app/services/project.service';
import { TokenService } from 'src/app/services/token.service';
import { ProjectEditUserPageModel } from 'src/app/shared/project/user-on-project/edit/project-edit-user-page.model';
import { UpdateUserOnProjectModel } from 'src/app/shared/project/user-on-project/edit/update-user-on-project.model';
import { RoleWithDescription } from 'src/app/shared/project/user-on-project/role/role-with-description.model';
import { UserOnProjectRole } from 'src/app/shared/project/user-on-project/role/user-on-project-role';
import { UserOnProjectExtendedModel } from 'src/app/shared/project/user-on-project/user-on-project-extended.model';

@Component({
  selector: 'app-project-user-edit',
  templateUrl: './project-user-edit.component.html',
  styleUrls: ['./project-user-edit.component.scss']
})
export class ProjectUserEditComponent implements OnInit {
  loading: boolean = false;
  success: boolean = false;
  errorMessage: string | null | undefined;
  private _projectId: number;
  projectName: string;
  user: UserOnProjectExtendedModel;
  myRole: UserOnProjectRole;
  roles: RoleWithDescription[] = [];
  form: FormGroup = null!;
  @ViewChild('eform') formDirective: any;
  selectedRole: RoleWithDescription;

  constructor(private _dialogRef: MatDialogRef<ProjectUserEditComponent>, 
    @Inject(MAT_DIALOG_DATA) public data: ProjectEditUserPageModel, private _router: Router,
    private _projectService: ProjectService, private _tokenService: TokenService, public roleService: ProjectRoleService,
    private _fb: FormBuilder) { 
      this._projectId = data.project.id;
      this.projectName = data.project.name;
      this.user = data.user;
      this.myRole = data.myRole;
      this.roles = this.roleService.getRolesWithDescription(this.myRole);
      this.selectedRole = this.roles[0];
      this.createForm();
  }

  createForm() {
    const currentRole = this.roleService.roleToString(this.user.role);
    let roleControl = new FormControl(currentRole);
    this.form = this._fb.group({
      role: roleControl
    });
    roleControl.valueChanges
    .subscribe(value => this.changeSelectedRole(value));
    this.changeSelectedRole(currentRole);
  }

  ngOnInit(): void {
  }

  onSubmit(): void {
    this.loading = true;
    let model: UpdateUserOnProjectModel = this.form.value;
    this._projectService.updateUser(this._tokenService.getJwtToken()!, {
      id: this._projectId, 
      userId: this.user.user.id
    }, model).subscribe(
    {
      next: () => {
        this.success = true;
        this.loading = false;
        this.user.role = this.roleService.roleToEnum(model.role);
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

}

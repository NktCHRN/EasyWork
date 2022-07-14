import { Injectable } from '@angular/core';
import { RoleWithDescription } from '../shared/project/user-on-project/role/role-with-description.model';
import { UserOnProjectRole } from '../shared/project/user-on-project/role/user-on-project-role';

@Injectable({
  providedIn: 'root'
})
export class ProjectRoleService {

  constructor() { }

  public roleToString(role: UserOnProjectRole): string {
    return UserOnProjectRole[role];
  }

  public roleToEnum(role: string): UserOnProjectRole {
    return UserOnProjectRole[role as keyof typeof UserOnProjectRole];
  }

  public isKickable(myRole: UserOnProjectRole, modelRole: UserOnProjectRole): boolean
  {
    if (myRole == UserOnProjectRole.Owner)
      return true;
    if (myRole == UserOnProjectRole.Manager && modelRole < UserOnProjectRole.Manager)
      return true;
    return false;
  }

  public getRolesWithDescription(myRole: UserOnProjectRole): RoleWithDescription[]
  {
    let result = [
      {
        role: UserOnProjectRole.User,
        description: 'User can participate in project: view information, other participants, an invite-code; add, edit and delete tasks'
      }
    ];
    if (myRole == UserOnProjectRole.Owner)
    {
      result = result.concat([
        {
          role: UserOnProjectRole.Manager,
          description: 'Manager has all user\'s abilities and also ' +
          'can update an invite-code and its\' status, add users, kick participants with role "User", delete tags of the whole project and change tasks\' limits'
        },
        {
          role: UserOnProjectRole.Owner,
          description: 'Owner has full access to the project, including abilities to change the project information, to delete the project, ' +
          'to add, edit or kick any user. Be careful with granting this role!'
        }
      ]);
    };
    return result
  }
}

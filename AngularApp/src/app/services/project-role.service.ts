import { Injectable } from '@angular/core';
import { UserOnProjectRole } from '../shared/project/role/user-on-project-role';

@Injectable({
  providedIn: 'root'
})
export class ProjectRoleService {

  constructor() { }

  public roleToString(role: UserOnProjectRole): string {
    return UserOnProjectRole[role];
  }

  public isKickable(myRole: UserOnProjectRole, modelRole: UserOnProjectRole): boolean
  {
    if (myRole == UserOnProjectRole.Owner)
      return true;
    if (myRole == UserOnProjectRole.Manager && modelRole < UserOnProjectRole.Manager)
      return true;
    return false;
  }
}

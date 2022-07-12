import { TestBed } from '@angular/core/testing';

import { ProjectRoleService } from './project-role.service';

describe('ProjectRoleService', () => {
  let service: ProjectRoleService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ProjectRoleService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});

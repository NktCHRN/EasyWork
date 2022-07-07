import { TestBed } from '@angular/core/testing';

import { InvitesService } from './invites.service';

describe('InvitesService', () => {
  let service: InvitesService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(InvitesService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});

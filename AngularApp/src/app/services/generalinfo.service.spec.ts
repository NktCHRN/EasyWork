import { TestBed } from '@angular/core/testing';

import { GeneralinfoService } from './generalinfo.service';

describe('GeneralinfoService', () => {
  let service: GeneralinfoService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(GeneralinfoService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});

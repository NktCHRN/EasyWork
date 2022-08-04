import { TestBed } from '@angular/core/testing';

import { BanService } from './ban.service';

describe('BanService', () => {
  let service: BanService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(BanService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});

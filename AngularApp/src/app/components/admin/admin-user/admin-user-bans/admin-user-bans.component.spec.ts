import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdminUserBansComponent } from './admin-user-bans.component';

describe('AdminUserBansComponent', () => {
  let component: AdminUserBansComponent;
  let fixture: ComponentFixture<AdminUserBansComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AdminUserBansComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AdminUserBansComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

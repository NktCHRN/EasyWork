import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProjectInviteRegenerateComponent } from './project-invite-regenerate.component';

describe('ProjectInviteRegenerateComponent', () => {
  let component: ProjectInviteRegenerateComponent;
  let fixture: ComponentFixture<ProjectInviteRegenerateComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ProjectInviteRegenerateComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ProjectInviteRegenerateComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

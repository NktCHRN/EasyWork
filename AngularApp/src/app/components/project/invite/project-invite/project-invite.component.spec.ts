import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProjectInviteComponent } from './project-invite.component';

describe('ProjectInviteComponent', () => {
  let component: ProjectInviteComponent;
  let fixture: ComponentFixture<ProjectInviteComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ProjectInviteComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ProjectInviteComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

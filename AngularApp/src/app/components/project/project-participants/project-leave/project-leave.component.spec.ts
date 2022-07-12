import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProjectLeaveComponent } from './project-leave.component';

describe('ProjectLeaveComponent', () => {
  let component: ProjectLeaveComponent;
  let fixture: ComponentFixture<ProjectLeaveComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ProjectLeaveComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ProjectLeaveComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

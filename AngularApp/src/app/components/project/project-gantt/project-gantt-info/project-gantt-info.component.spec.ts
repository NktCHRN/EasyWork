import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProjectGanttInfoComponent } from './project-gantt-info.component';

describe('ProjectGanttInfoComponent', () => {
  let component: ProjectGanttInfoComponent;
  let fixture: ComponentFixture<ProjectGanttInfoComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ProjectGanttInfoComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ProjectGanttInfoComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

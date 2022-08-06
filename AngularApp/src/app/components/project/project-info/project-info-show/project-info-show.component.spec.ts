import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProjectInfoShowComponent } from './project-info-show.component';

describe('ProjectInfoShowComponent', () => {
  let component: ProjectInfoShowComponent;
  let fixture: ComponentFixture<ProjectInfoShowComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ProjectInfoShowComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ProjectInfoShowComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProjectInfoEditComponent } from './project-info-edit.component';

describe('ProjectInfoEditComponent', () => {
  let component: ProjectInfoEditComponent;
  let fixture: ComponentFixture<ProjectInfoEditComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ProjectInfoEditComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ProjectInfoEditComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

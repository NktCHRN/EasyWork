import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProjectUserEditComponent } from './project-user-edit.component';

describe('ProjectUserEditComponent', () => {
  let component: ProjectUserEditComponent;
  let fixture: ComponentFixture<ProjectUserEditComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ProjectUserEditComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ProjectUserEditComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

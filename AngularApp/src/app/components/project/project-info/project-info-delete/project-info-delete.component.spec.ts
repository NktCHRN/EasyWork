import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProjectInfoDeleteComponent } from './project-info-delete.component';

describe('ProjectInfoDeleteComponent', () => {
  let component: ProjectInfoDeleteComponent;
  let fixture: ComponentFixture<ProjectInfoDeleteComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ProjectInfoDeleteComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ProjectInfoDeleteComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

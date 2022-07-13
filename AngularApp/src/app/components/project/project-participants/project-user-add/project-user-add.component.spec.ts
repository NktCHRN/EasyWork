import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProjectUserAddComponent } from './project-user-add.component';

describe('ProjectUserAddComponent', () => {
  let component: ProjectUserAddComponent;
  let fixture: ComponentFixture<ProjectUserAddComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ProjectUserAddComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ProjectUserAddComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

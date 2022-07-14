import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProjectTagDeleteComponent } from './project-tag-delete.component';

describe('ProjectTagDeleteComponent', () => {
  let component: ProjectTagDeleteComponent;
  let fixture: ComponentFixture<ProjectTagDeleteComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ProjectTagDeleteComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ProjectTagDeleteComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

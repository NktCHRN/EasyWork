import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TaskFilesComponent } from './task-files.component';

describe('TaskFilesComponent', () => {
  let component: TaskFilesComponent;
  let fixture: ComponentFixture<TaskFilesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ TaskFilesComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(TaskFilesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

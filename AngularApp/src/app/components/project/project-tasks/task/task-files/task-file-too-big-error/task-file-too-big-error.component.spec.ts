import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TaskFileTooBigErrorComponent } from './task-file-too-big-error.component';

describe('TaskFileTooBigErrorComponent', () => {
  let component: TaskFileTooBigErrorComponent;
  let fixture: ComponentFixture<TaskFileTooBigErrorComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ TaskFileTooBigErrorComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(TaskFileTooBigErrorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

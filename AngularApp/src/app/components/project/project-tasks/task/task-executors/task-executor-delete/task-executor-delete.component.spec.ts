import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TaskExecutorDeleteComponent } from './task-executor-delete.component';

describe('TaskExecutorDeleteComponent', () => {
  let component: TaskExecutorDeleteComponent;
  let fixture: ComponentFixture<TaskExecutorDeleteComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ TaskExecutorDeleteComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(TaskExecutorDeleteComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

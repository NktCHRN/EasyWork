import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TaskExecutorsComponent } from './task-executors.component';

describe('TaskExecutorsComponent', () => {
  let component: TaskExecutorsComponent;
  let fixture: ComponentFixture<TaskExecutorsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ TaskExecutorsComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(TaskExecutorsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

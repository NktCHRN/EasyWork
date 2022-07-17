import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TaskMessageDeleteComponent } from './task-message-delete.component';

describe('TaskMessageDeleteComponent', () => {
  let component: TaskMessageDeleteComponent;
  let fixture: ComponentFixture<TaskMessageDeleteComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ TaskMessageDeleteComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(TaskMessageDeleteComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

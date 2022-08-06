import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TaskReducedComponent } from './task-reduced.component';

describe('TaskReducedComponent', () => {
  let component: TaskReducedComponent;
  let fixture: ComponentFixture<TaskReducedComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ TaskReducedComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(TaskReducedComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

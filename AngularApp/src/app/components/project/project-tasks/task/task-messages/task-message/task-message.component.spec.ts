import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TaskMessageComponent } from './task-message.component';

describe('TaskMessageComponent', () => {
  let component: TaskMessageComponent;
  let fixture: ComponentFixture<TaskMessageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ TaskMessageComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(TaskMessageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

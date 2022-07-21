import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TaskFileDeleteComponent } from './task-file-delete.component';

describe('TaskFileDeleteComponent', () => {
  let component: TaskFileDeleteComponent;
  let fixture: ComponentFixture<TaskFileDeleteComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ TaskFileDeleteComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(TaskFileDeleteComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

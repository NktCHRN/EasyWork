import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TaskFileUploadCancelComponent } from './task-file-upload-cancel.component';

describe('TaskFileUploadCancelComponent', () => {
  let component: TaskFileUploadCancelComponent;
  let fixture: ComponentFixture<TaskFileUploadCancelComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ TaskFileUploadCancelComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(TaskFileUploadCancelComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TaskMessagesComponent } from './task-messages.component';

describe('TaskMessagesComponent', () => {
  let component: TaskMessagesComponent;
  let fixture: ComponentFixture<TaskMessagesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ TaskMessagesComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(TaskMessagesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

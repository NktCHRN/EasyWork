import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AvatarEditComponent } from './avatar-edit.component';

describe('AvatarEditComponent', () => {
  let component: AvatarEditComponent;
  let fixture: ComponentFixture<AvatarEditComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AvatarEditComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AvatarEditComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

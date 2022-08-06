import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AvatarDeleteComponent } from './avatar-delete.component';

describe('AvatarDeleteComponent', () => {
  let component: AvatarDeleteComponent;
  let fixture: ComponentFixture<AvatarDeleteComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AvatarDeleteComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AvatarDeleteComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

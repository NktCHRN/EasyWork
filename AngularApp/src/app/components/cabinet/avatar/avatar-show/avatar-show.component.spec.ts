import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AvatarShowComponent } from './avatar-show.component';

describe('AvatarShowComponent', () => {
  let component: AvatarShowComponent;
  let fixture: ComponentFixture<AvatarShowComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AvatarShowComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AvatarShowComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

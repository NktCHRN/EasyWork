import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AvatarBaseComponent } from './avatar-base.component';

describe('AvatarBaseComponent', () => {
  let component: AvatarBaseComponent;
  let fixture: ComponentFixture<AvatarBaseComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AvatarBaseComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AvatarBaseComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

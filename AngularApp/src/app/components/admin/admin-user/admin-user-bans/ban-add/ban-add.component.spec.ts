import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BanAddComponent } from './ban-add.component';

describe('BanAddComponent', () => {
  let component: BanAddComponent;
  let fixture: ComponentFixture<BanAddComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ BanAddComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(BanAddComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

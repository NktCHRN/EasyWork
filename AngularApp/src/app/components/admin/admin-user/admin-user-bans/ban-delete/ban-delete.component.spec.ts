import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BanDeleteComponent } from './ban-delete.component';

describe('BanDeleteComponent', () => {
  let component: BanDeleteComponent;
  let fixture: ComponentFixture<BanDeleteComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ BanDeleteComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(BanDeleteComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

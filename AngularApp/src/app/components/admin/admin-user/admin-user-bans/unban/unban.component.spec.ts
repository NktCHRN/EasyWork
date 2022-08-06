import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UnbanComponent } from './unban.component';

describe('UnbanComponent', () => {
  let component: UnbanComponent;
  let fixture: ComponentFixture<UnbanComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ UnbanComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(UnbanComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

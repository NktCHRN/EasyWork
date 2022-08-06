import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProjectKickComponent } from './project-kick.component';

describe('ProjectKickComponent', () => {
  let component: ProjectKickComponent;
  let fixture: ComponentFixture<ProjectKickComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ProjectKickComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ProjectKickComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

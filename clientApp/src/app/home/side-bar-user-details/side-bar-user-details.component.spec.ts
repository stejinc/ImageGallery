import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SideBarUserDetailsComponent } from './side-bar-user-details.component';

describe('SideBarUserDetailsComponent', () => {
  let component: SideBarUserDetailsComponent;
  let fixture: ComponentFixture<SideBarUserDetailsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SideBarUserDetailsComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(SideBarUserDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

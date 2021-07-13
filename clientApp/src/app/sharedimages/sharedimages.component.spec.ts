import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SharedimagesComponent } from './sharedimages.component';

describe('SharedimagesComponent', () => {
  let component: SharedimagesComponent;
  let fixture: ComponentFixture<SharedimagesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SharedimagesComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(SharedimagesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

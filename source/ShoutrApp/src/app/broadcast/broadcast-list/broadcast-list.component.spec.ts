import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BroadcastListComponent } from './broadcast-list.component';

describe('BroadcastListComponent', () => {
  let component: BroadcastListComponent;
  let fixture: ComponentFixture<BroadcastListComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [BroadcastListComponent]
    });
    fixture = TestBed.createComponent(BroadcastListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BroadcastItemComponent } from './broadcast-item.component';

describe('BroadcastItemComponent', () => {
  let component: BroadcastItemComponent;
  let fixture: ComponentFixture<BroadcastItemComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [BroadcastItemComponent]
    });
    fixture = TestBed.createComponent(BroadcastItemComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

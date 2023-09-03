import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SelectablePeerComponent } from './selectable-peer.component';

describe('SelectablePeerComponent', () => {
  let component: SelectablePeerComponent;
  let fixture: ComponentFixture<SelectablePeerComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [SelectablePeerComponent]
    });
    fixture = TestBed.createComponent(SelectablePeerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PeerListComponent } from './peer-list.component';

describe('PeerListComponent', () => {
  let component: PeerListComponent;
  let fixture: ComponentFixture<PeerListComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [PeerListComponent]
    });
    fixture = TestBed.createComponent(PeerListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

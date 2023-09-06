import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PeerIconComponent } from './peer-icon.component';

describe('PeerIconComponent', () => {
  let component: PeerIconComponent;
  let fixture: ComponentFixture<PeerIconComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [PeerIconComponent]
    });
    fixture = TestBed.createComponent(PeerIconComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should create', () => {
    component.stringToIcon("3482DJSOFgjfd39420");
    
    //expect(component).toBeTruthy();
  });
});

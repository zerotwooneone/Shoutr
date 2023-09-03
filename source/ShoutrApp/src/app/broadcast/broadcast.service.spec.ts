import { TestBed } from '@angular/core/testing';

import { BroadcastService } from './broadcast.service';

describe('PeerService', () => {
  let service: BroadcastService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(BroadcastService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});

import { TestBed } from '@angular/core/testing';
import { CanActivateFn } from '@angular/router';

import { connectedGuard } from './connected.guard';

describe('connectedGuard', () => {
  const executeGuard: CanActivateFn = (...guardParameters) => 
      TestBed.runInInjectionContext(() => connectedGuard(...guardParameters));

  beforeEach(() => {
    TestBed.configureTestingModule({});
  });

  it('should be created', () => {
    expect(executeGuard).toBeTruthy();
  });
});

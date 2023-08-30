import { inject } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivateFn, Router, RouterStateSnapshot } from '@angular/router';
import { BackendService } from './backend.service';

export const connectedGuard: CanActivateFn = (
  next: ActivatedRouteSnapshot,
  state: RouterStateSnapshot) => {
  let backendService = inject(BackendService);
  if (!backendService.Connected) {
    let router = inject(Router);
    return router.navigate(['']);
  }
  return true;
};

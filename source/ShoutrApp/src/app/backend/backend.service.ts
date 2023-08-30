import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, delay, firstValueFrom, of } from 'rxjs';
import { BackendModule } from './backend.module';

@Injectable({
  providedIn: BackendModule
})
export class BackendService {
  private readonly _connecting: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
  public readonly Connecting$: Observable<boolean>;
  get Connecting(): boolean { return this._connecting.value; }
  private readonly _connected: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
  public readonly Connected$: Observable<boolean>;
  get Connected(): boolean { return this._connected.value; }
  constructor() {
    this.Connecting$ = this._connecting.asObservable();
    this.Connected$ = this._connected.asObservable();
  }

  public async connect(): Promise<Observable<boolean>> {
    if (this.Connecting || this.Connected) {
      return of(false);
    }
    this._connecting.next(true);
    await firstValueFrom(of(1).pipe(delay(300)));
    this._connecting.next(false);
    this._connected.next(true);

    return of(true);
  }

  public async disconnect(): Promise<Observable<boolean>> {
    if (!this.Connected) {
      return of(false);
    }
    await delay(30);
    this._connecting.next(false);
    this._connected.next(false);

    return of(true);
  }


}

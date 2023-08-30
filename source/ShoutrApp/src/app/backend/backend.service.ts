import { Injectable } from '@angular/core';
import { BehaviorSubject, EMPTY, Observable, delay, firstValueFrom, mergeMap, of } from 'rxjs';
import { BackendModule } from './backend.module';
import { BackendConfig, BackendModel } from './backend-config';

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
  private readonly _config: BehaviorSubject<BackendModel> = new BehaviorSubject<BackendModel>({});
  public readonly Config$: Observable<BackendConfig>;
  get Config(): BackendConfig | undefined {
    let config = this._config.value;
    if (this.Validate(config)) {
      return <BackendConfig>config;
    }
    return undefined;
  }
  constructor() {
    this.Connecting$ = this._connecting.asObservable();
    this.Connected$ = this._connected.asObservable();
    this.Config$ = this._config.asObservable().pipe(mergeMap(c => {
      if (this.Validate(c)) {
        return of(<BackendConfig>c);
      }
      return EMPTY;
    }))
  }

  public async connect(): Promise<Observable<boolean>> {
    if (this.Connecting || this.Connected) {
      return of(false);
    }
    this._connecting.next(true);

    //pretend to connect
    await firstValueFrom(of(1).pipe(delay(300)));
    this._connecting.next(false);
    this._connected.next(true);

    //pretend to get the config
    await firstValueFrom(of(1).pipe(delay(80)));
    this._config.next({});

    return of(true);
  }

  public async disconnect(): Promise<Observable<boolean>> {
    if (!this.Connected) {
      return of(false);
    }
    await firstValueFrom(of(1).pipe(delay(300)));
    this._connecting.next(false);
    this._connected.next(false);

    return of(true);
  }

  private Validate(config: BackendModel): boolean {
    if (!config) {
      return false;
    }
    return !!config.UserFingerprint && !!config.UserPublicKey;
  }


}

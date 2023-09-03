import { Injectable } from '@angular/core';
import { BehaviorSubject, EMPTY, Observable, concat, delay, firstValueFrom, mergeMap, of } from 'rxjs';
import { BackendModule } from './backend.module';
import { BackendConfig, BackendModel } from './backend-config';
import { Hub } from './hub/hub';
import { HubPeer } from './hub/Peer';
import { Peer } from './Peer';
import { Broadcast } from './Broadcast';

@Injectable({
  providedIn: BackendModule
})
export class BackendService {
  private readonly _connecting: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
  public readonly Connecting$: Observable<boolean>;
  private readonly _hub: Hub;
  get Connecting(): boolean { return this._connecting.value; }
  private readonly _connected: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
  public readonly Connected$: Observable<boolean>;
  get Connected(): boolean { return this._connected.value; }
  private readonly _config: BehaviorSubject<BackendModel> = new BehaviorSubject<BackendModel>({});
  public readonly Config$: Observable<BackendConfig>;
  public readonly PeerChanged$: Observable<Peer>;
  public readonly BroadcastChanged$: Observable<Broadcast>;
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
    }));
    this._hub = new Hub("frontend");
    this.PeerChanged$ = this._hub.PeerChanged$.pipe(
      mergeMap(hubPeer => {
        const peer = this.ConvertPeer(hubPeer);
        if (!peer) {
          return EMPTY;
        }
        return of(peer);
      })
    );
    this.BroadcastChanged$ = concat(
      of(<Broadcast>{ id: "first" }),
      of(<Broadcast>{ id: "second" }).pipe(delay(300)),
      of(<Broadcast>{ id: "third" }).pipe(delay(1300))) 
  }
  ConvertPeer(hubPeer: HubPeer): Peer | undefined {
    if (!hubPeer?.id) {
      return undefined;
    }
    return {
      id: hubPeer.id,
      nickname: hubPeer.nickname,
      publicKey: hubPeer.publicKey
    };
  }

  public async connect(): Promise<Observable<boolean>> {
    if (this.Connecting || this.Connected) {
      return of(false);
    }
    this._connecting.next(true);

    //try to connect
    await this._hub.Start();
    //todo: abort if start fails
    this._connecting.next(false);
    this._connected.next(true);

    //pretend to get the config
    await firstValueFrom(of(1).pipe(delay(80)));
    this._config.next({
      UserFingerprint: "UserFingerprint",
      UserPublicKey: "UserPublicKey dflkjsdlkfja;slkdjflaskdjfaslkdjf;laskdjfalskdjfalskdjf;alskdjf;alksdjflak faslkd jflksdjflaksd jflksj dlfkja sldkfj askldjflksjdf;lksj"
    });

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



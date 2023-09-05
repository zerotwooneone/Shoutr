import { Injectable } from '@angular/core';
import { EMPTY, Observable, delay, firstValueFrom, mergeMap, of } from 'rxjs';
import { BackendModule } from './backend.module';
import { BackendConfig, BackendModel } from './backend-config';
import { Hub } from './hub/hub';
import { Peer } from './Peer';
import { Broadcast } from './Broadcast';
import { HubBroadcast, HubPeer } from './hub/hubTypes';
import { ObservableProperty } from '../util/observable-property';
import { IReadonlyObservableProperty } from '../util/IReadonlyObservableProperty';

@Injectable({
  providedIn: BackendModule
})
export class BackendService {
  private readonly _connecting$: ObservableProperty<boolean> = new ObservableProperty<boolean>(false);
  get Connecting$(): IReadonlyObservableProperty<boolean> { return this._connecting$; }
  private readonly _hub: Hub;
  get Connecting(): boolean { return this._connecting$.Value; }
  private readonly _connected$: ObservableProperty<boolean> = new ObservableProperty<boolean>(false);
  get Connected$(): IReadonlyObservableProperty<boolean> { return this._connected$; }
  get Connected(): boolean { return this._connected$.Value; }
  private readonly _config$: ObservableProperty<BackendModel> = new ObservableProperty<BackendModel>({});
  public readonly Config$: Observable<BackendConfig>;
  public readonly PeerChanged$: Observable<Peer>;
  public readonly BroadcastChanged$: Observable<Broadcast>;
  get Config(): BackendConfig | undefined {
    let config = this._config$.Value;
    if (this.Validate(config)) {
      return <BackendConfig>config;
    }
    return undefined;
  }
  constructor() {
    this.Config$ = this._config$.Value$.pipe(mergeMap(c => {
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
    this.BroadcastChanged$ = this._hub.BroadcastChanged$.pipe(
      mergeMap(hubBc => {
        const bc = this.ConvertBc(hubBc);
        if (!bc) {
          return EMPTY;
        }
        return of(bc);
      })
    )
  }
  ConvertBc(hubBc: HubBroadcast): Broadcast | undefined {
    if (!hubBc?.id) {
      return undefined;
    }
    return {
      id: hubBc.id,
      completed: hubBc.completed,
      percentComplete: hubBc.percentComplete
    };
  }
  ConvertPeer(hubPeer: HubPeer): Peer | undefined {
    if (!hubPeer?.id || !hubPeer?.nickname) {
      return undefined;
    }
    return {
      id: hubPeer.id,
      nickname: hubPeer.nickname,
      publicKey: hubPeer.publicKey
    };
  }

  public async connect(): Promise<boolean> {
    if (this.Connecting || this.Connected) {
      return false;
    }
    this._connecting$.Value = true;

    //try to connect
    try {
      await this._hub.Start();
    } finally {
      this._connecting$.Value = false;
    }

    this._connected$.Value = true;

    //pretend to get the config
    await firstValueFrom(of(1).pipe(delay(80)));
    this._config$.Value = {
      UserFingerprint: "UserFingerprint",
      UserPublicKey: "UserPublicKey dflkjsdlkfja;slkdjflaskdjfaslkdjf;laskdjfalskdjfalskdjf;alskdjf;alksdjflak faslkd jflksdjflaksd jflksj dlfkja sldkfj askldjflksjdf;lksj"
    };

    return true;
  }

  public async disconnect(): Promise<boolean> {
    if (!this.Connected) {
      return false;
    }
    await firstValueFrom(of(1).pipe(delay(300)));
    this._connecting$.Value = false;
    this._connected$.Value = false;

    return true;
  }

  private Validate(config: BackendModel): boolean {
    if (!config) {
      return false;
    }
    return !!config.UserFingerprint && !!config.UserPublicKey;
  }
}



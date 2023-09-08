import { Injectable } from '@angular/core';
import {
  EMPTY,
  Observable,
  Subject,
  concat,
  delay,
  filter,
  firstValueFrom,
  mergeMap,
  of,
  range,
  takeUntil,
  shareReplay
} from 'rxjs';
import { BackendModule } from './backend.module';
import { Hub } from './hub/hub';
import { Peer } from './Peer';
import { Broadcast } from './Broadcast';
import { HubBroadcast, HubPeer } from './hub/hubTypes';
import { ObservableProperty } from '../util/observable-property';
import { IReadonlyObservableProperty } from '../util/IReadonlyObservableProperty';
import { HubConfig } from './HubConfig';

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
  public readonly PeerChanged$: Observable<Peer>;
  public readonly BroadcastChanged$: Observable<Broadcast>;
  public readonly HubConfig$: Observable<HubConfig>;

  private readonly fakeBroadcasts = new Subject<Broadcast>();
  private readonly cancelFake = new Subject<string>();

  constructor() {
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
    );
    this.HubConfig$ = this._hub.ConfigChanged$.pipe(
      mergeMap(hc => {
        if (!hc?.userId || !hc?.userPublicKey) {
          return EMPTY;
        }
        return of({
          userId: hc.userId,
          userPublicKey: hc.userPublicKey,
        });
      }),
      shareReplay(1)
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
  public Download(id: string): boolean {

    concat(
      of(<Broadcast>{ id: id }).pipe(delay(1300)),
      range(0, 101).pipe(
        mergeMap(i => of(<Broadcast>{ id: id, percentComplete: i }).pipe(delay(30)), 1)
      ),
      of(<Broadcast>{ id: id, completed: true }).pipe(delay(300)),)
      .pipe(
        takeUntil(this.cancelFake.pipe(filter(b => b === id)))
      )
      .subscribe(bc => this.fakeBroadcasts.next(bc));
    return true;
  }

  UserCancel(id: string): boolean {
    this.cancelFake.next(id);
    return true;
  }
}



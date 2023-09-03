import { Injectable } from '@angular/core';
import { BackendService } from '../backend/backend.service';
import { Peer } from '../backend/Peer';
import { BehaviorSubject, Observable, Subject } from 'rxjs';
import { PeerModel } from '../peer/peer-model';
import { BroadcastModel } from './broadcast-model';
import { Broadcast } from '../backend/Broadcast';

@Injectable({
  providedIn: 'root'
})
export class BroadcastService {
  private readonly _knownPeers: Map<string, PeerModel> = new Map<string, PeerModel>();
  public readonly knownPeers$: Observable<PeerModel[]>;
  private readonly _knownPeers$: BehaviorSubject<PeerModel[]>;

  get knownBroadcasts$(): Observable<BroadcastModel[]> { return this._knownBroadcasts$.asObservable(); }
  private readonly _knownBroadcasts$: BehaviorSubject<BroadcastModel[]>;
  private readonly _knownBroadcasts: Map<string, BroadcastModel>;

  constructor(private readonly backendService: BackendService) {
    this._knownPeers$ = new BehaviorSubject<PeerModel[]>([]);
    this.knownPeers$ = this._knownPeers$.asObservable();   

    this._knownBroadcasts$ = new BehaviorSubject<BroadcastModel[]>([]);
    this._knownBroadcasts = new Map<string, BroadcastModel>();

    backendService.PeerChanged$.subscribe(this.OnPeerChanged.bind(this));
    backendService.BroadcastChanged$.subscribe(this.OnBroadcastChanged.bind(this));
  }
  private OnPeerChanged(peer: Peer) {
    const found = this._knownPeers.get(peer.id);
    if (!found) {
      const newPeer = new PeerModel(peer.id, peer.nickname, peer.publicKey);
      this._knownPeers.set(peer.id, newPeer);
      this._knownPeers$.next(Array.from(this._knownPeers.values()));
      return;
    }
    found.setNickname(peer.nickname);
    if (peer.publicKey) {
      found.setPublicKey(peer.publicKey);
    }
  }

  private OnBroadcastChanged(bc: Broadcast) {
    const found = this._knownBroadcasts.get(bc.id);
    if (!found) {
      if (bc.completed) {
        return;
      }
      const newBc = new BroadcastModel(bc.id);
      this._knownBroadcasts.set(bc.id, newBc);
      this._knownBroadcasts$.next(Array.from(this._knownBroadcasts.values()));
      return;
    }
    if (bc.completed) {
      this._knownBroadcasts.delete(bc.id);
      this._knownBroadcasts$.next(Array.from(this._knownBroadcasts.values()));
    }
  }
}

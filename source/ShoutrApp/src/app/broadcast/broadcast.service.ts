import { Injectable } from '@angular/core';
import { BackendService, PeerX } from '../backend/backend.service';
import { BehaviorSubject, Observable, Subject } from 'rxjs';
import { PeerModel } from '../peer/peer-model';

@Injectable({
  providedIn: 'root'
})
export class BroadcastService {
  private readonly _knownPeers: Map<string, PeerModel> = new Map<string, PeerModel>();
  public readonly knownPeers$: Observable<PeerModel[]>;
  private readonly _knownPeers$: BehaviorSubject<PeerModel[]>;
  constructor(private readonly backendService: BackendService) {
    this._knownPeers$ = new BehaviorSubject(<PeerModel[]>[]);
    this.knownPeers$ = this._knownPeers$.asObservable();

    backendService.PeerChanged$.subscribe(this.OnPeerChanged.bind(this));
  }
  OnPeerChanged(peer: PeerX) {
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
}

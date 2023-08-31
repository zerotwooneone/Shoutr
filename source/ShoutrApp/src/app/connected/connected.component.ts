import { Component } from '@angular/core';
import { BackendService, PeerX } from '../backend/backend.service';
import { Observable } from 'rxjs';
import { BackendConfig } from '../backend/backend-config';

@Component({
  selector: 'zh-connected',
  templateUrl: './connected.component.html',
  styleUrls: ['./connected.component.scss']
})
export class ConnectedComponent {
  readonly Config: Observable<BackendConfig>;
  readonly Peers: PeerX[] = [];
  constructor(private readonly backendService: BackendService) {
    this.Config = backendService.Config$;
    this.backendService.PeerChanged$.subscribe(this.OnPeerChanged);
  }
  OnPeerChanged(peer: PeerX) {
    const found = this.Peers.find(existing => existing.id === peer.id);
    if (!found) {
      this.Peers.push(peer);
      return;
    }
    found.nickname = peer.nickname;
    found.publicKey = peer.publicKey;
  }
}

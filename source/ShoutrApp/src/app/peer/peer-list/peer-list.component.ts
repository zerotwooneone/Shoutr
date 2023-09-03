import { Component } from '@angular/core';
import { PeerModel } from '../peer-model';
import { BroadcastService } from 'src/app/broadcast/broadcast.service';

@Component({
  selector: 'zh-peer-list',
  templateUrl: './peer-list.component.html',
  styleUrls: ['./peer-list.component.scss']
})
export class PeerListComponent {
  constructor(readonly peerService: BroadcastService) { }

  trackPeer(index: number, peer: PeerModel) {
    return peer?.id;
  }

}

import { Component, Input } from '@angular/core';
import { PeerModel } from '../peer-model';

@Component({
  selector: 'zh-selectable-peer',
  templateUrl: './selectable-peer.component.html',
  styleUrls: ['./selectable-peer.component.scss']
})
export class SelectablePeerComponent {
  @Input() peer?: PeerModel;
}

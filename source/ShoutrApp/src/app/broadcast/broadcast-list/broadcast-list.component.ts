import { Component } from '@angular/core';
import { BroadcastService } from '../broadcast.service';
import { BroadcastModel } from '../broadcast-model';

@Component({
  selector: 'zh-broadcast-list',
  templateUrl: './broadcast-list.component.html',
  styleUrls: ['./broadcast-list.component.scss']
})
export class BroadcastListComponent {
  constructor(readonly broadcastService: BroadcastService) { }
  trackBroadcast(index: number, broadcast: BroadcastModel) {
    return broadcast.id;
  }
}

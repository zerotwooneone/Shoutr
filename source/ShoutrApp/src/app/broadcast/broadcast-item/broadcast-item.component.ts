import { Component, Input } from '@angular/core';
import { BroadcastModel } from '../broadcast-model';

@Component({
  selector: 'zh-broadcast-item',
  templateUrl: './broadcast-item.component.html',
  styleUrls: ['./broadcast-item.component.scss']
})
export class BroadcastItemComponent {
  @Input() broadcast?: BroadcastModel;
}

import { Component } from '@angular/core';
import { BackendService } from '../backend/backend.service';

@Component({
  selector: 'zh-connected',
  templateUrl: './connected.component.html',
  styleUrls: ['./connected.component.scss']
})
export class ConnectedComponent {
  constructor(readonly backendService: BackendService) {

  }

}

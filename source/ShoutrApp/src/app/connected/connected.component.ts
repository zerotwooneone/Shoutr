import { Component } from '@angular/core';
import { BackendService } from '../backend/backend.service';
import { Peer } from '../backend/Peer';
import { Observable } from 'rxjs';
import { BackendConfig } from '../backend/backend-config';

@Component({
  selector: 'zh-connected',
  templateUrl: './connected.component.html',
  styleUrls: ['./connected.component.scss']
})
export class ConnectedComponent {
  readonly Config: Observable<BackendConfig>;
  constructor(private readonly backendService: BackendService) {
    this.Config = backendService.Config$;
  }

}

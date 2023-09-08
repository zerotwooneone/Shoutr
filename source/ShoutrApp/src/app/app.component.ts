import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { BackendService } from './backend/backend.service';

@Component({
  selector: 'zh-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  constructor(
    private router: Router,
    private backendService: BackendService) {

  }
  ngOnInit(): void {
    //todo: change this to an app initializer
    this.backendService.connect()
      .then(async didConnect => {
        if (!didConnect) {
          console.warn("we did not connect");
          return;
        }
        this.router.navigate(['/connected']);
      })
      .catch(reason => {
        console.error(`error connecting reason:${reason}`);
      });
  }
  title = 'ShoutrApp';
}

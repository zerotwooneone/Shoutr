import { Component, Input, OnChanges, OnDestroy, SimpleChanges } from '@angular/core';
import { BroadcastModel } from '../broadcast-model';
import { Observable, Subject, map, of, takeUntil, tap } from 'rxjs';
import { ProgressBarMode } from '@angular/material/progress-bar';
import { BroadcastService } from '../broadcast.service';

@Component({
  selector: 'zh-broadcast-item',
  templateUrl: './broadcast-item.component.html',
  styleUrls: ['./broadcast-item.component.scss']
})
export class BroadcastItemComponent implements OnChanges, OnDestroy {
  @Input() broadcast?: BroadcastModel;
  private behaviorTeardown$: Subject<void>;
  progressMode$: Observable<ProgressBarMode>;
  progressVisible$: Observable<boolean>;
  downloadVisible$: Observable<boolean>;
  cancelVisible$: Observable<boolean>;
  constructor(private readonly broadcastService: BroadcastService) {
    this.progressMode$ = of("indeterminate");
    this.progressVisible$ = of(false);
    this.downloadVisible$ = of(false);
    this.cancelVisible$ = of(false);
    this.behaviorTeardown$ = new Subject<void>();
  }
  ngOnDestroy(): void {
    this.behaviorTeardown$.next();
  }
  ngOnChanges(changes: SimpleChanges): void {
    if (changes['broadcast']?.currentValue) {
      //kill any existing subscriptions
      this.behaviorTeardown$.next();
      const tearDown = this.behaviorTeardown$.asObservable();

      const broadcast = <BroadcastModel>changes['broadcast'].currentValue;
      this.progressMode$ = broadcast.percentComplete$.Value$.pipe(
        map(pct => (!!pct)
          ? "determinate"
          : "indeterminate"),
        takeUntil(tearDown)
      );
      this.progressVisible$ = broadcast.downloadState$.Value$.pipe(
        map(state => {
          switch (state) {
            case "in progress":
            case "started but unknown":
              return true;
            case "source stopped":
            case "user stopped":
            case "not started":
            default:
              return false;
          }
        }),
        takeUntil(tearDown)
      );
      this.downloadVisible$ = broadcast.downloadState$.Value$.pipe(
        map(state => {
          switch (state) {
            case "user stopped":
            case "not started":
              return true;
            case "in progress":
            case "started but unknown":
            case "source stopped":
            default:
              return false;
          }
        }),
        takeUntil(tearDown)
      );
      this.cancelVisible$ = broadcast.downloadState$.Value$.pipe(
        map(state => {
          switch (state) {
            case "in progress":
            case "started but unknown":
              return true;
            case "user stopped":
            case "not started":
            case "source stopped":
            default:
              return false;
          }
        }),
        takeUntil(tearDown)
      );
    }
  }
  DownloadClick() {
    if (!this.broadcast) {
      return;
    }
    this.broadcastService.Download(this.broadcast.id);
  }
  StopClick() {
    if (!this.broadcast) {
      return;
    }
    this.broadcastService.UserCancel(this.broadcast.id);
  }
}

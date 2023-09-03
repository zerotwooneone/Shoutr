import { Component, Input, OnChanges, OnDestroy, SimpleChanges } from '@angular/core';
import { BroadcastModel } from '../broadcast-model';
import { BehaviorSubject, Observable, Subject, map, of, takeUntil } from 'rxjs';
import { ProgressBarMode } from '@angular/material/progress-bar';

@Component({
  selector: 'zh-broadcast-item',
  templateUrl: './broadcast-item.component.html',
  styleUrls: ['./broadcast-item.component.scss']
})
export class BroadcastItemComponent implements OnChanges, OnDestroy {
  @Input() broadcast?: BroadcastModel;
  private behaviorTeardown$: Subject<void>;
  progressMode$: Observable<ProgressBarMode>;
  constructor() {
    this.progressMode$ = of("indeterminate");
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
    }
  }
}

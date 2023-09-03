import { IReadonlyObservableProperty } from "../util/IReadonlyObservableProperty";
import { ObservableProperty } from "../util/observable-property";

export class BroadcastModel {

    get percentComplete$(): IReadonlyObservableProperty<number | undefined> { return this._percentComplete$; }
    private readonly _percentComplete$: ObservableProperty<number | undefined>;

    get downloadState$(): IReadonlyObservableProperty<DownloadState> { return this._downloadState$; }
    private readonly _downloadState$: ObservableProperty<DownloadState>;

    constructor(readonly id: string,
        percentComplete?: number,
        downloadState?: DownloadState) {
        this._percentComplete$ = new ObservableProperty<number | undefined>(percentComplete);
        this._downloadState$ = new ObservableProperty<DownloadState>(downloadState ?? "not started");
    }
    SetPercentComplete(percentComplete: number | undefined) {
        this._percentComplete$.Value = percentComplete;
    }
    SetDownloadState(state: DownloadState) {
        //todo: add state transition checking
        this._downloadState$.Value = state;
    }
}

export type DownloadState = "not started" | "started but unknown" | "in progress" | "source stopped" | "user stopped";

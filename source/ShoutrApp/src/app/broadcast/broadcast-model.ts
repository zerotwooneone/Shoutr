import { IReadonlyObservableProperty } from "../util/IReadonlyObservableProperty";
import { ObservableProperty } from "../util/observable-property";

export class BroadcastModel {

    get percentComplete$(): IReadonlyObservableProperty<number | undefined> { return this._percentComplete$; }
    private readonly _percentComplete$: ObservableProperty<number | undefined>;
    get percentComplete(): number | undefined { return this._percentComplete$.Value; }

    constructor(readonly id: string) {
        this._percentComplete$ = new ObservableProperty<number | undefined>(undefined);
    }
    SetPercentComplete(percentComplete: number | undefined) {
        this._percentComplete$.Value = percentComplete;
    }
}

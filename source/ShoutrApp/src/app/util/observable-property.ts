import { BehaviorSubject, Observable, skip } from "rxjs";
import { IReadonlyObservableProperty } from "./IReadonlyObservableProperty";


export class ObservableProperty<T> implements IReadonlyObservableProperty<T> {
    private readonly _subject: BehaviorSubject<T>;
    get Value(): T {
        return this._subject.value;
    }
    set Value(v: T) {
        if (v === this._subject.value) {
            return;
        }
        this._subject.next(v);
    }
    public readonly Value$: Observable<T>;
    public readonly Change$: Observable<T>;
    constructor(value: T) {
        this._subject = new BehaviorSubject<T>(value);
        this.Value$ = this._subject.asObservable();
        this.Change$ = this.Value$.pipe(skip(1));
    }
}
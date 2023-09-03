import { Observable } from "rxjs";


export interface IReadonlyObservableProperty<T> {
    /**Gets the most recent value of the property */
    readonly Value: T;
    /**Returns the most recent value of the property and all future changes to the value */
    readonly Value$: Observable<T>;
    /**Returns only when the value changes; This does NOT include the current value */
    readonly Change$: Observable<T>;
}

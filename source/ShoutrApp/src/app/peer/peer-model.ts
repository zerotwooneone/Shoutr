import { IReadonlyObservableProperty } from "../util/IReadonlyObservableProperty";
import { ObservableProperty } from "../util/observable-property";

export class PeerModel {
    public readonly nickname$: IReadonlyObservableProperty<string>;
    private _nickname$: ObservableProperty<string>;

    public readonly publicKey$: IReadonlyObservableProperty<string | undefined>;
    private readonly _publicKey$: ObservableProperty<string | undefined>;

    get known$(): IReadonlyObservableProperty<boolean> { return this._known$; }
    private readonly _known$: ObservableProperty<boolean>;
    get known(): boolean { return this._known$.Value; }

    constructor(
        public readonly id: string,
        nickname: string,
        publicKey?: string,
        known: boolean = false) {

        this._nickname$ = new ObservableProperty(nickname);
        this.nickname$ = this._nickname$;

        this._publicKey$ = new ObservableProperty<string | undefined>(publicKey);
        this.publicKey$ = this._publicKey$;

        this._known$ = new ObservableProperty(known);
    }
    public toggleKnown(): boolean {
        this._known$.Value = !this._known$.Value;
        return this.known$.Value;
    }
    public setNickname(nickname: string): void {
        this._nickname$.Value = nickname;
    }
    setPublicKey(publicKey: string) {
        if (!publicKey) {
            throw new Error(`Invalid public key: ${publicKey}`);
        }
        this._publicKey$.Value = publicKey;
    }
}

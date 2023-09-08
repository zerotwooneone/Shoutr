import * as signalR from "@microsoft/signalr"
import { NEVER, Observable, Subject, concat, delay, interval, map, merge, mergeMap, of, range } from "rxjs";
import { environment } from "src/environments/environment";
import { HubBroadcast, HubConfig, HubPeer } from "./hubTypes";

export class Hub {
    private readonly _connection: signalR.HubConnection;
    private readonly _handlers: handler2[];
    public readonly PeerChanged$: Observable<HubPeer>;
    public readonly BroadcastChanged$: Observable<HubBroadcast>;
    public readonly ConfigChanged$: Observable<HubConfig>;

    /**todo:delete this*/
    private readonly useFakeData: boolean = true;    

    constructor(private readonly hubName: string) {
        const logLevel = environment.production
            ? signalR.LogLevel.Warning
            : signalR.LogLevel.Information;
        this._connection = new signalR.HubConnectionBuilder()
            .configureLogging(logLevel)
            .withUrl(environment.baseUrl + this.hubName)
            .build();
        const sendConfigHandler = this.GetHandler<HubConfig>("SendConfigToClient");
        const peerchangedHandler = this.GetHandler<HubPeer>("PeerChanged");
        const broadcastchangedHandler = this.GetHandler<HubBroadcast>("BroadcastChanged");

        this.PeerChanged$ = this.useFakeData
          ? this.GetFakePeerChanged()
          : peerchangedHandler.observable;
        this.BroadcastChanged$ = this.useFakeData
            ? this.GetFakeBroadcastData()
            : broadcastchangedHandler.observable;
        this.ConfigChanged$ = this.useFakeData
            ? this.GetFakeConfig()
            : sendConfigHandler.observable;
        this._handlers = [
            peerchangedHandler,
            broadcastchangedHandler,
            sendConfigHandler,
        ];
    }
    private GetFakeConfig(): Observable<HubConfig> {
        return concat(of({
            userId: "Fake User Id",
            userPublicKey: "UserPublicKey dflkjsdlkfja;slkdjflaskdjfaslkdjf;laskdjfalskdjfalskdjf;alskdjf;alksdjflak faslkd jflksdjflaksd jflksj dlfkja sldkfj askldjflksjdf;lksj"
        }),
            NEVER);
    }
    private GetFakeBroadcastData(): Observable<HubBroadcast> {
        return concat(
            of(<HubBroadcast>{ id: "first" }),
            of(<HubBroadcast>{ id: "second" }).pipe(delay(1300)),
            of(<HubBroadcast>{ id: "third" }).pipe(delay(900)),
            range(0, 100).pipe(
                mergeMap(
                    i => of(<HubBroadcast>{ id: "second", percentComplete: i }).pipe(delay(300)),
                    1)
            ),
            of(<HubBroadcast>{ id: "second", completed: true }).pipe(delay(300)),
            of(<HubBroadcast>{ id: "third", completed: true }).pipe(delay(1300)))
    }
    private GetFakePeerChanged(): Observable<HubPeer> {
        return interval(1000).pipe(
            map(n => <HubPeer>{ id: "some id", nickname: "This is the user's nickname", publicKey: "fdlksfsljkfdsjlkfdj sfj sdfjsdfj;sdfjkdfj fsdjkldfj sdfjdsflk sdjdsjkfsj dfsfd f" })
        );
    }

    GetHandler<T>(
        methodName: string,
        validator?: (...args: any[]) => boolean,
        selector?: (...args: any[]) => T | undefined): handler<T> {
        const subject = new Subject<T>();
        return {
            methodName: methodName,
            handler: this.GetHandlerCallback(subject, validator, selector),
            observable: subject.asObservable(),
        };
    }
    GetHandlerCallback<T>(
        subject: Subject<T>,
        validator?: (...args: any[]) => boolean,
        selector?: (...args: any[]) => T | undefined): (...args: any[]) => any {
        const realValidator = validator ?? this.defaultValidator;
        const realSelector = selector ?? this.defaultSelector<T>;
        return (...args: any[]) => {
            if (!realValidator(...args)) {
                //todo:log this
                return;
            }
            const t = realSelector(...args);
            if (!t) {
                //todo:log this
                return;
            }
            subject.next(t);
        }

    }
    defaultSelector<T>(...args: any[]): T | undefined {
        if (!args || !args.length) {
            return undefined;
        }
        return <T>args[0];
    }
    defaultValidator(...args: any[]): boolean {
        if (!args || !args.length) {
            return false;
        }
        return true;
    }
    public async Start(): Promise<void> {
        if (this.useFakeData) {
            await of(1).pipe(delay(80));
            return;
        }
        await this._connection
            .start()
            .then(() => console.debug('SignalR Started'));
        for (const handler of this._handlers) {
            if (!handler) {
                continue;
            }
            if (!handler.methodName) {
                continue;
            }
            if (!handler.handler) {
                continue;
            }
            this._connection.on(handler.methodName, handler.handler)
        }
    }
}

interface handler2 {
    methodName: string;
    handler: (...args: any[]) => any;
}
interface handler<T> extends handler2 {
    observable: Observable<T>;
}


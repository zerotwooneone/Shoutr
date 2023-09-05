import * as signalR from "@microsoft/signalr"
import { Observable, Subject, interval, map, merge } from "rxjs";
import { environment } from "src/environments/environment";
import { HubPeer } from "./hubTypes";

export class Hub {
    private readonly _connection: signalR.HubConnection;
    private readonly _handlers: handler2[];
    public readonly PeerChanged$: Observable<HubPeer>;
    constructor(private readonly hubName: string) {
        const logLevel = environment.production
            ? signalR.LogLevel.Warning
            : signalR.LogLevel.Information;
        this._connection = new signalR.HubConnectionBuilder()
            .configureLogging(logLevel)
            .withUrl(environment.baseUrl + this.hubName)
            .build();
        const peerchangedHandler = this.GetHandler<HubPeer>("PeerChanged");

        const useFakeData = false;

        this.PeerChanged$ = useFakeData
          ? this.GetFakePeerChanged()
          : peerchangedHandler.observable;
        this._handlers = [
            peerchangedHandler
        ];
    }
    GetFakePeerChanged(): Observable<HubPeer> {
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
        await this._connection
            .start()
            .then(() => console.debug('SignalR Started'))
            .catch(reason => console.error(`SignalR Error:${reason}`));
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


import * as signalR from '@microsoft/signalr';

export class ConnectionContainer {
    connection: signalR.HubConnection = undefined!;
    id: string | null = null;    
}

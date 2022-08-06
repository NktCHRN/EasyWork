import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';

@Injectable({
  providedIn: 'root'
})
export class ConnectionService {

  constructor() { }

  public getOpenConnection(connection: signalR.HubConnection) {
    return new Promise((resolve, reject) => {
        const msMax = 10000;
        const msInc = 10;

        var ms = 0;

        var idInterval = setInterval(() => {
            if (connection.state == signalR.HubConnectionState.Connected) {
                clearInterval(idInterval);
                resolve(connection);
            }
            
            ms += msInc;

            if (ms >= msMax) {
                clearInterval(idInterval);
                reject(connection);
            }
        }, msInc);
    });
  }
}

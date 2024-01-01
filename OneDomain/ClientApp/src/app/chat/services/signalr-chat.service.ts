import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr'

@Injectable({
  providedIn: 'root'
})
export class SignalrChatService {
  private hubConnection: signalR.HubConnection | undefined

  public startConnection() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:7194/chat')
      .build()

    this.hubConnection
      .start()
      .then(() => console.log("Connection started"))
      .catch(err => console.log(`Error while starting connection: ${ err }`))
  }

  public addChatMsgListener(callback: (user: string, text: string) => void) {
    this.hubConnection?.on('Receive', callback)
  }

  public sendMessage(user: string, text: string, restartCall: () => void) {
    this.hubConnection?.invoke('SendMessageAsync', user, text)
      .catch(err => {
        console.log(`Error while sending message: ${ err }`)
        restartCall()
      })
  }

  stopConnection() {
    this.hubConnection?.stop()
      .catch(err => {
        console.log(`Error while stopping hub: ${ err }`)
      })
  }
}

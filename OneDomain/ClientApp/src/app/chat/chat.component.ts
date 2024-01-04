import { ChangeDetectorRef, Component, SecurityContext } from '@angular/core';
import { FormControl, FormGroup, Validators } from "@angular/forms";
import { DomSanitizer } from "@angular/platform-browser";
import { SignalrChatService } from "./services/signalr-chat.service";
import { AuthorizeService } from "../../api-authorization/authorize.service";
import { map } from "rxjs/operators";
import { HttpClient } from "@angular/common/http";
import { Subscription } from "rxjs";

@Component({
  selector: 'app-chat',
  templateUrl: './chat.component.html',
  styleUrls: [ './chat.component.css' ]
})
export class ChatComponent {
  messages: IMessage[] = [];
  private _user: string | null = null;
  private getMsgSub!: Subscription;
  private userSub: Subscription = new Subscription;

  sendMessageForm = new FormGroup({
    'text': new FormControl('', Validators.required)
  });

  constructor(private readonly chatService: SignalrChatService,
              private readonly changeDetector: ChangeDetectorRef,
              private readonly httpClient: HttpClient,
              private readonly authService: AuthorizeService,
              private readonly sanitizer: DomSanitizer) {
  }

  ngOnInit() {
    this.getMsgSub = this.httpClient.get<IMessage[]>("https://localhost:7194/chat-users/messages")
      .subscribe(value => {
        this.messages = value
        console.log(`Fetched Messages ${ this.messages.length }: ${ this.messages.join(', ') }`);
      });

    this.initConnection();
  }

  ngOnDestroy(): void {
    this.chatService.stopConnection();
    this.getMsgSub.unsubscribe();
    this.userSub.unsubscribe();
  }

  public sendMessage() {
    let text = this.sendMessageForm.get('text')?.value ?? '<empty>';
    text = this.sanitizer.sanitize(SecurityContext.HTML, text) || '';
    this.chatService.sendMessage(<string> this._user, text, () => {
      alert('Error while trying sending message! Try again later');
      this.initConnection();
    });
    this.sendMessageForm.reset();
  }

  get user(): string {
    if (this._user === null) {
      let userObs = this.authService.getUser()
        .pipe(map(user => user?.name));

      this.userSub = userObs.subscribe(value => this._user = value ?? null);
    }

    return <string> this._user;
  }

  private initConnection() {
    this.chatService.startConnection();
    this.chatService.addChatMsgListener((user, text) => {
      this.messages.push({ userId: '', messageText: text, user: { userName: user } });
      this.changeDetector.detectChanges();
    });
  }
}

interface IMessage {
  userId: string
  messageText: string
  user: IUser
}

interface IUser {
  userName: string
}

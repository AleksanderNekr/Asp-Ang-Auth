import { ChangeDetectorRef, Component, SecurityContext } from '@angular/core';
import { FormControl, FormGroup, Validators } from "@angular/forms";
import { DomSanitizer } from "@angular/platform-browser";
import { SignalrChatService } from "./services/signalr-chat.service";

@Component({
  selector: 'app-chat',
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.css']
})
export class ChatComponent {
  messages: Message[] = [];
  private _user = '';

  inputNameForm = new FormGroup({
    'user': new FormControl('', Validators.required)
  });
  sendMessageForm = new FormGroup({
    'text': new FormControl('', Validators.required)
  });

  constructor(private readonly chatService: SignalrChatService,
              private readonly changeDetector: ChangeDetectorRef,
              private readonly sanitizer: DomSanitizer) {
  }

  ngOnInit() {
    this.chatService.startConnection();
    this.chatService.addChatMsgListener((user, text) => {
      this.messages.push({ user, text });
      this.changeDetector.detectChanges();
    });
  }

  ngOnDestroy(): void {
    this.chatService.stopConnection()
  }

  public sendMessage() {
    let text = this.sendMessageForm.get('text')?.value ?? '<empty>';
    text = this.sanitizer.sanitize(SecurityContext.HTML, text) || '';
    this.chatService.sendMessage(this.user, text, () => {
      alert('Error while trying sending message! Try again later');
      this.ngOnInit();
    });
    this.sendMessageForm.reset();
  }

  set user(value: string | null | undefined) {
    if (this._user !== value) {
      this._user = value ?? '<empty>';
      this.changeDetector.detectChanges();
    }
  }

  get user(): string {
    return this._user;
  }

  saveUser() {
    this.user = this.inputNameForm.get('user')?.value;
  }
}

interface Message {
  user: string
  text: string
}

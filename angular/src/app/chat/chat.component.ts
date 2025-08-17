import { CoreModule } from '@abp/ng.core'; // *ngFor ve *ngIf icin gerekli
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ChatService } from './chat.service';


@Component({
  selector: 'app-chat', 
  standalone: true,
  imports: [
    CoreModule, 
    FormsModule 
  ],
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.scss']
})
export class ChatComponent {
  userInput: string = '';
  messages: { role: string; message: string }[] = [];

  constructor(private chatService: ChatService) {
    console.log('ChatComponent YÜKLENDİ');
  }

  sendMessage() {
    if (!this.userInput.trim()) {  // trim stringin basındaki ve sonundaki boslukları siler ve yani bos bir string varsa send yapmaz
      return;
    }

    // Kullanıcı mesajını ekle
    this.messages.push({
      role: 'user',
      message: this.userInput.trim()
    });

    console.log('Kullanıcı mesajı alındı');

    const dto = { 
      role : 'user',
      message: this.userInput.trim() 
    };

    console.log('DTO JSON:', JSON.stringify(dto));  

    this.chatService.askChatGpt(dto).subscribe({
      next: response => {
        console.log('✅ Bot cevabı alındı:', response);
        this.messages.push({ role: 'assistant', message: response });
      },
      error: err => {
        console.error('❌ Backend hatası:', err);
      }
    });

    // Input'u temizle
    this.userInput = '';
  }
}

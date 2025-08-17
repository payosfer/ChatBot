import { Component } from '@angular/core';
import { CoreModule } from '@abp/ng.core'; 
import { FormsModule } from '@angular/forms';
import { ChatContextService } from './chat-context.service';

@Component({
  standalone: true,
  selector: 'app-chat-context',
  imports: [CoreModule, FormsModule],
  templateUrl: './chat-context.component.html',
  styleUrl: './chat-context.component.scss'
})
export class ChatContextComponent {
  userInput: string = '';
  messages: { role: string; message: string }[] = [];
  selectedModel: string =  "DeepSeek"; // Varsayılan model

  constructor(private chatService: ChatContextService) {}

  sendMessage() {
    if (!this.userInput.trim()) return;

    // Kullanıcı mesajını ekle
    this.messages.push({
      role: 'user',
      message: this.userInput.trim()
    });

    console.log('Kullanıcı mesajı alındı');

    const dto = { 
      role : 'user',
      message: this.userInput.trim(),
      model: this.selectedModel 
    };

    console.log('DTO JSON:', JSON.stringify(dto));  

    this.chatService.ask(dto).subscribe({
      next: response => {
        console.log('✅ Bot cevabı alındı:', response);
        this.messages.push({ 
          role: 'assistant',   // ????????
          message: response });
      },
      error: err => {
        console.error('❌ Backend hatası:', err);
      }
    });
    // Input'u temizle
    this.userInput = '';
  }
}

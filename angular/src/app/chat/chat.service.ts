// Angular frontend ve backend deki OpenAiAppService arasında bağlantı kurar

import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({ providedIn: 'root' })
export class ChatService {
  constructor(private http: HttpClient) {}

  askChatGpt(dto: { message: string }) {
    return this.http.post('/api/chat/ask', dto, {
      responseType: 'text'  // 🔥 bu satır gerekli
    });
  }
}

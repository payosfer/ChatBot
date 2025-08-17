import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class ChatContextService {
  constructor(private http: HttpClient) {}

  ask(dto: { message: string; model?: string }) {
    return this.http.post('/api/chatcontext/ask', dto, {
      responseType: 'text'  // 🔥 bu satır gerekli
    });
  }
}

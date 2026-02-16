import { Injectable, NgZone } from '@angular/core';
import { LiveMessage } from '../models/live-message.model';
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class LiveSocketService {
  private socket?: WebSocket;
  private messageSubject = new Subject<LiveMessage>();

  messages$ = this.messageSubject.asObservable();

  constructor(private zone: NgZone) {}

  connect(): void {
    if (this.socket) return;

    this.socket = new WebSocket('ws://localhost:5053/dashboard');

    this.socket.onopen = () => {
      console.log('WebSocket connected');
    };

    this.socket.onmessage = (event) => {
      this.zone.run(() => {
        const data = JSON.parse(event.data) as LiveMessage;
        this.messageSubject.next(data);
      });
    };

    this.socket.onclose = () => {
      console.log('WebSocket disconnected');
      this.socket = undefined;
    };

    this.socket.onerror = (err) => {
      console.error('WebSocket error', err);
    };
  }

  disconnect(): void {
    this.socket?.close();
    this.socket = undefined;
  }
}
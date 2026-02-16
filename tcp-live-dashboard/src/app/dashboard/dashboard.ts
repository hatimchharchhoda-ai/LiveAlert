import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { LiveSocketService } from '../services/live-socket-service';
import { Subscription } from 'rxjs';
import { LiveMessage } from '../models/live-message.model';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-dashboard',
  imports: [CommonModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class Dashboard implements OnInit, OnDestroy {
  messages: LiveMessage[] = [];
  processedMessage: LiveMessage[] =[];
  totalMessages = 0;
  perClientCount: Record<string, number> = {};

  private sub?: Subscription;
  private cleanupInterval : any;

  constructor(private socketService: LiveSocketService, private cdr: ChangeDetectorRef) { }

  ngOnInit(): void {
    this.socketService.connect();

    this.sub = this.socketService.messages$.subscribe(raw => {
      
      let receivedTime = new Date(raw.ReceivedAt);

      const msg: LiveMessage = {
        ClientEndpoint: raw.ClientEndpoint,
        Content: raw.Content,
        ReceivedAt: receivedTime,
        ThreadId: raw.ThreadId,
        isProcessed: raw.isProcessed,
        Priority: raw.Priority || ""
      };
      // console.log(raw.isProcessed);
      
      if(raw.Priority != "") {
        // console.log("inside");
        
        this.processedMessage.unshift(msg);
      } else {
        this.messages.unshift(msg);
      }
      console.log(this.processedMessage);
      
      this.cdr.detectChanges();
    });


    // Clean up after 60 sec
    this.cleanupInterval = setInterval(() => {
      const now = new Date().getTime();

      this.messages = this.messages.filter(msg => {
        
        if (!msg.ReceivedAt) return false;

        const msgTime = new Date(msg.ReceivedAt).getTime();
        return (now - msgTime) <= 60000;
      });

      this.cdr.detectChanges();
    }, 1000);
  }


  ngOnDestroy(): void {
    this.sub?.unsubscribe();
    this.socketService.disconnect();
    clearInterval(this.cleanupInterval)
  }
}
export interface LiveMessage {
  ClientEndpoint: string;
  Content: string;
  ReceivedAt: Date;
  ThreadId: number;
  isProcessed: boolean;
  Priority?: string;
}
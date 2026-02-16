import { TestBed } from '@angular/core/testing';

import { LiveSocketService } from './live-socket-service';

describe('LiveSocketService', () => {
  let service: LiveSocketService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(LiveSocketService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});

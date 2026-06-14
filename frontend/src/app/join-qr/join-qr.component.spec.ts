import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import { JoinQrComponent } from './join-qr.component';

vi.mock('qrcode', () => ({
  default: {
    toDataURL: vi.fn().mockResolvedValue('data:image/png;base64,join-qr-code')
  }
}));

describe('JoinQrComponent', () => {
  let canvasGetContextSpy: ReturnType<typeof vi.spyOn>;

  beforeEach(async () => {
    canvasGetContextSpy = vi
      .spyOn(HTMLCanvasElement.prototype, 'getContext')
      .mockReturnValue(null);

    await TestBed.configureTestingModule({
      imports: [JoinQrComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([])
      ]
    }).compileComponents();
  });

  afterEach(() => {
    TestBed.inject(HttpTestingController).verify();
    canvasGetContextSpy.mockRestore();
  });

  it('should render a printable join QR poster page', async () => {
    const baseElement = document.querySelector('base') ?? document.head.appendChild(document.createElement('base'));
    const originalBaseHref = baseElement.getAttribute('href') ?? '/';
    baseElement.setAttribute('href', '/QueueManagementApp/');
    const fixture = TestBed.createComponent(JoinQrComponent);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();

    flushTodayQueue(http);
    await flushAsyncWork();
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h1')?.textContent).toContain('Printable join QR');
    expect(compiled.textContent).toContain('Priya Dental Clinic');
    expect(compiled.textContent).toContain('AB7K2M9Q');
    expect(compiled.textContent).toContain(`${globalThis.location.origin}/QueueManagementApp/join/AB7K2M9Q`);
    expect(compiled.textContent).toContain('Download image');
    expect(compiled.textContent).toContain('Print poster');

    baseElement.setAttribute('href', originalBaseHref);
  });

  it('should open a printable poster window', async () => {
    const fixture = TestBed.createComponent(JoinQrComponent);
    const http = TestBed.inject(HttpTestingController);
    const printWindow = {
      document: {
        open: vi.fn(),
        write: vi.fn(),
        close: vi.fn()
      },
      focus: vi.fn(),
      print: vi.fn()
    };
    const openSpy = vi.spyOn(globalThis, 'open').mockReturnValue(printWindow as unknown as Window);
    fixture.detectChanges();

    flushTodayQueue(http);
    await flushAsyncWork();
    fixture.detectChanges();

    fixture.debugElement.query(By.css('.poster-actions .secondary-button')).triggerEventHandler('click');

    expect(openSpy).toHaveBeenCalled();
    expect(printWindow.document.write).toHaveBeenCalledWith(expect.stringContaining('Join Queue QR - AB7K2M9Q'));
    expect(printWindow.document.write).toHaveBeenCalledWith(expect.stringContaining('data:image/png'));
    expect(printWindow.print).toHaveBeenCalled();

    openSpy.mockRestore();
  });

  function flushTodayQueue(http: HttpTestingController): void {
    const request = http.expectOne('http://localhost:5020/api/manager/queue/today');
    expect(request.request.method).toBe('GET');
    request.flush({
      queueLocationId: 1,
      locationCode: 'AB7K2M9Q',
      businessName: 'Priya Dental Clinic',
      isQueueOpen: true,
      waitingCount: 0,
      currentCalled: null,
      waitingEntries: [],
      skippedEntries: [],
      recentServedEntries: []
    });
  }

  function flushAsyncWork(): Promise<void> {
    return new Promise(resolve => setTimeout(resolve, 0));
  }
});

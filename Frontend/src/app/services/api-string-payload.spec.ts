import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { API_BASE_URL } from '../config/api.config';
import { CommercialService } from './commercial.service';
import { PrintJobService } from './print-job.service';
import { PrinterService } from './printer.service';

describe('API string payload services', () => {
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        CommercialService,
        PrintJobService,
        PrinterService,
      ],
    });

    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('sends printer status as a JSON string', () => {
    const service = TestBed.inject(PrinterService);

    service.updateStatus(12, 'Available').subscribe();

    const req = httpMock.expectOne(`${API_BASE_URL}/Printer/12/status`);
    expect(req.request.method).toBe('PATCH');
    expect(req.request.body).toBe('"Available"');
    expect(req.request.headers.get('Content-Type')).toBe('application/json');
    req.flush({});
  });

  it('sends print job failure reason as a JSON string', () => {
    const service = TestBed.inject(PrintJobService);

    service.fail(4, 'Nozzle clogged').subscribe();

    const req = httpMock.expectOne(`${API_BASE_URL}/PrintJob/4/fail`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toBe('"Nozzle clogged"');
    expect(req.request.headers.get('Content-Type')).toBe('application/json');
    req.flush({});
  });

  it('sends commercial order status as a JSON string', () => {
    const service = TestBed.inject(CommercialService);

    service.updateCommandeStatut(9, 'Livrée').subscribe();

    const req = httpMock.expectOne(`${API_BASE_URL}/Commercial/commandes/9/statut`);
    expect(req.request.method).toBe('PATCH');
    expect(req.request.body).toBe('"Livrée"');
    expect(req.request.headers.get('Content-Type')).toBe('application/json');
    req.flush({});
  });
});

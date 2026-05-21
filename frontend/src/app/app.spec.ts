import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { RouterOutlet } from '@angular/router';
import { By } from '@angular/platform-browser';
import { App } from './app';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideRouter([])]
    }).compileComponents();
  });

  it('should create the app shell', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;

    expect(app).toBeTruthy();
  });

  it('should render only the router outlet at the shell level', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();

    expect(fixture.debugElement.query(By.directive(RouterOutlet))).not.toBeNull();
    expect((fixture.nativeElement as HTMLElement).querySelector('form')).toBeNull();
  });
});

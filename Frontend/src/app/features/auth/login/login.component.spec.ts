import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { of, throwError } from 'rxjs';
import { LoginComponent } from './login.component';
import { AuthService } from '../../../core/services/auth.service';
const mockSuccess = { success: true, message: 'Giriş başarılı.', data: { accessToken: 'tok', expiresAt: new Date() } };
const mockFail    = { success: false, message: 'Hatalı şifre.', data: null };
describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let authSpy: jasmine.SpyObj<AuthService>;
  beforeEach(async () => {
    authSpy = jasmine.createSpyObj('AuthService', ['login', 'isLoggedIn']);
    authSpy.isLoggedIn.and.returnValue(false);
    authSpy.login.and.returnValue(of(mockSuccess as any));
    await TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [
        provideHttpClient(),
        provideRouter([]),
        provideNoopAnimations(),
        { provide: AuthService, useValue: authSpy }
      ]
    }).compileComponents();
    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });
  it('bileşen oluşturulmalı', () => {
    expect(component).toBeTruthy();
  });
  it('form başlangıçta geçersiz olmalı (boş alanlar)', () => {
    expect(component.loginForm?.valid).toBeFalse();
  });
  it('geçerli email ve şifre ile form geçerli olmalı', () => {
    component.loginForm?.setValue({ email: 'test@test.com', password: 'Test1234!' });
    expect(component.loginForm?.valid).toBeTrue();
  });
  it('geçersiz email ile form geçersiz olmalı', () => {
    component.loginForm?.setValue({ email: 'not-an-email', password: 'Test1234!' });
    expect(component.loginForm?.valid).toBeFalse();
  });
});

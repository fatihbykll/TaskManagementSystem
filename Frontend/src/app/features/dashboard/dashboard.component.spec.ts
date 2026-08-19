import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { of } from 'rxjs';
import { DashboardComponent } from './dashboard.component';
import { TaskService } from '../../core/services/task.service';
const mockStats = {
  success: true, message: '', data: {
    totalTasks: 10, pendingCount: 3, inProgressCount: 2,
    completedCount: 4, cancelledCount: 1, overdueCount: 2, completedThisMonth: 3
  }
};
const mockOverdue = { success: true, message: '', data: [] };
describe('DashboardComponent', () => {
  let component: DashboardComponent;
  let fixture: ComponentFixture<DashboardComponent>;
  let taskServiceSpy: jasmine.SpyObj<TaskService>;
  beforeEach(async () => {
    taskServiceSpy = jasmine.createSpyObj('TaskService', ['getStatistics', 'getOverdue']);
    taskServiceSpy.getStatistics.and.returnValue(of(mockStats as any));
    taskServiceSpy.getOverdue.and.returnValue(of(mockOverdue as any));
    await TestBed.configureTestingModule({
      imports: [DashboardComponent],
      providers: [
        provideHttpClient(),
        provideRouter([]),
        provideNoopAnimations(),
        { provide: TaskService, useValue: taskServiceSpy }
      ]
    }).compileComponents();
    fixture = TestBed.createComponent(DashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });
  it('bileşen oluşturulmalı', () => {
    expect(component).toBeTruthy();
  });
  it('istatistikler yüklendiğinde statCards doldurulmalı', () => {
    expect(component.statCards.length).toBe(6);
  });
  it('toplam görev sayısı doğru gösterilmeli', () => {
    expect(component.stats?.totalTasks).toBe(10);
  });
  it('overdue görevler yüklendiğinde liste boş olmalı', () => {
    expect(component.overdueTasks.length).toBe(0);
  });
  it('loading tamamlandığında isLoading false olmalı', () => {
    expect(component.isLoading).toBeFalse();
  });
});

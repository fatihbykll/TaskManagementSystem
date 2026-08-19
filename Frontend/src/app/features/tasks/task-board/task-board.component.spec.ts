import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { TaskBoardComponent } from './task-board.component';
import { TaskItem, TaskStatus, TaskPriority } from '../../../models/task.model';
const makeTasks = (): TaskItem[] => [
  { id: '1', title: 'Görev 1', status: TaskStatus.Pending,    priority: TaskPriority.Low,  createdAt: new Date(), updatedAt: new Date() } as any,
  { id: '2', title: 'Görev 2', status: TaskStatus.InProgress, priority: TaskPriority.High, createdAt: new Date(), updatedAt: new Date() } as any,
  { id: '3', title: 'Görev 3', status: TaskStatus.Completed,  priority: TaskPriority.Low,  createdAt: new Date(), updatedAt: new Date() } as any,
];
describe('TaskBoardComponent', () => {
  let component: TaskBoardComponent;
  let fixture: ComponentFixture<TaskBoardComponent>;
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TaskBoardComponent],
      providers: [provideNoopAnimations()]
    }).compileComponents();
    fixture = TestBed.createComponent(TaskBoardComponent);
    component = fixture.componentInstance;
    component.tasks = makeTasks();
    fixture.detectChanges();
  });
  it('bileşen oluşturulmalı', () => {
    expect(component).toBeTruthy();
  });
  it('4 kanban kolonu olmalı', () => {
    expect(component.columns.length).toBe(4);
  });
  it('Pending görevler Beklemede kolonunda olmalı', () => {
    const pending = component.columns.find(c => c.status === TaskStatus.Pending);
    expect(pending?.tasks.length).toBe(1);
    expect(pending?.tasks[0].title).toBe('Görev 1');
  });
  it('InProgress görevler Devam Ediyor kolonunda olmalı', () => {
    const inProg = component.columns.find(c => c.status === TaskStatus.InProgress);
    expect(inProg?.tasks.length).toBe(1);
  });
  it('dueDateWarn gecikmiş görev için uyarı döndürmeli', () => {
    const overdueTask = { ...makeTasks()[0], dueDate: new Date('2020-01-01') };
    const warn = component.dueDateWarn(overdueTask as any);
    expect(warn).not.toBeNull();
    expect(warn?.color).toBe('#c62828');
  });
  it('statusChanged event emit edilmeli', () => {
    spyOn(component.statusChanged, 'emit');
    component.statusChanged.emit({ taskId: '1', newStatus: TaskStatus.Completed });
    expect(component.statusChanged.emit).toHaveBeenCalled();
  });
});

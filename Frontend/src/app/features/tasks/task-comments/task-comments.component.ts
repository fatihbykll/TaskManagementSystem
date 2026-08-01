import { Component, Input, OnInit, inject, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TaskCommentService, TaskComment } from '../../../core/services/task-comment.service';
@Component({
  selector: 'app-task-comments',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule],
  template: `
    <div class="comments-section">
      <h3><mat-icon>forum</mat-icon> Yorumlar</h3>
      
      <div class="add-comment">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Yorum Ekle...</mat-label>
          <textarea matInput [formControl]="commentControl" rows="2" maxlength="500"></textarea>
          <mat-error *ngIf="commentControl.hasError('required')">Yorum boş olamaz.</mat-error>
        </mat-form-field>
        <button mat-flat-button color="primary" [disabled]="commentControl.invalid || isLoading" (click)="addComment()">
          <mat-icon *ngIf="!isLoading">send</mat-icon>
          <mat-spinner *ngIf="isLoading" diameter="20"></mat-spinner> Gönder
        </button>
      </div>
      <div class="comment-list">
        @for (comment of comments; track comment.id) {
          <div class="comment-item">
            <div class="comment-header">
              <span class="author">{{ comment.authorName }}</span>
              <span class="date">{{ comment.createdAt | date:'dd MMM yyyy HH:mm':'':'tr' }}</span>
            </div>
            <p class="comment-text">{{ comment.text }}</p>
          </div>
        }
        @if (comments.length === 0) {
          <p class="no-comments">Henüz yorum yapılmamış.</p>
        }
      </div>
    </div>
  `,
  styles: [`
    .comments-section { margin-top: 1.5rem; }
    h3 { display: flex; align-items: center; gap: 0.5rem; font-size: 1.1rem; color: #555; }
    .full-width { width: 100%; }
    .add-comment { display: flex; flex-direction: column; align-items: flex-end; gap: 0.5rem; margin-bottom: 1.5rem; }
    .comment-list { display: flex; flex-direction: column; gap: 1rem; }
    .comment-item { background: #f8f9fa; padding: 1rem; border-radius: 8px; border-left: 4px solid #667eea; }
    .comment-header { display: flex; justify-content: space-between; margin-bottom: 0.5rem; font-size: 0.85rem; }
    .author { font-weight: 600; color: #333; }
    .date { color: #888; }
    .comment-text { margin: 0; line-height: 1.5; color: #444; }
    .no-comments { color: #aaa; font-style: italic; }
  `]
})
export class TaskCommentsComponent implements OnInit {
  @Input({ required: true }) taskId!: string;
  
  private readonly commentService = inject(TaskCommentService);
  private readonly destroyRef = inject(DestroyRef); // Angular 16+ DestroyRef (takeUntilDestroyed için)
  comments: TaskComment[] = [];
  commentControl = new FormControl('', [Validators.required, Validators.maxLength(500)]);
  isLoading = false;
  ngOnInit() { this.loadComments(); }
  loadComments() {
    this.commentService.getComments(this.taskId)
      .pipe(takeUntilDestroyed(this.destroyRef)) // Component yok olunca memory leak'i engeller
      .subscribe(res => { if (res.success) this.comments = res.data; });
  }
  addComment() {
    if (this.commentControl.invalid) return;
    this.isLoading = true;
    this.commentService.addComment(this.taskId, this.commentControl.value!)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (res) => {
          if (res.success) {
            this.comments.unshift(res.data);
            this.commentControl.reset();
          }
          this.isLoading = false;
        },
        error: () => this.isLoading = false
      });
  }
}

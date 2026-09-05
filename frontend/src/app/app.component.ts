import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';

import { GameService } from './services/game.service';
import { GameMode, GameStateResponse } from './models/game.models';

import { BoardComponent } from './components/board/board.component';
import { ScoreboardComponent } from './components/scoreboard/scoreboard.component';
import { MoveHistoryComponent } from './components/move-history/move-history.component';
import { ModeSelectorComponent } from './components/mode-selector/mode-selector.component';
import { StatusBannerComponent } from './components/status-banner/status-banner.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    BoardComponent,
    ScoreboardComponent,
    MoveHistoryComponent,
    ModeSelectorComponent,
    StatusBannerComponent
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  game: GameStateResponse | null = null;
  errorMessage: string | null = null;
  busy = false;

  constructor(private readonly gameService: GameService) {}

  startGame(mode: GameMode): void {
    this.errorMessage = null;
    this.busy = true;
    this.gameService.createGame(mode).subscribe({
      next: (state) => {
        this.game = state;
        this.busy = false;
      },
      error: (err) => this.handleError(err)
    });
  }

  onCellChosen(cellIndex: number): void {
    if (!this.game || this.busy) return;
    this.errorMessage = null;
    this.busy = true;
    this.gameService.makeMove(this.game.gameId, this.game.currentPlayer, cellIndex).subscribe({
      next: (state) => {
        this.game = state;
        this.busy = false;
      },
      error: (err) => this.handleError(err)
    });
  }

  onUndo(): void {
    if (!this.game || this.busy) return;
    this.errorMessage = null;
    this.busy = true;
    this.gameService.undo(this.game.gameId).subscribe({
      next: (state) => {
        this.game = state;
        this.busy = false;
      },
      error: (err) => this.handleError(err)
    });
  }

  onResetBoard(): void {
    if (!this.game || this.busy) return;
    this.errorMessage = null;
    this.busy = true;
    this.gameService.resetGame(this.game.gameId).subscribe({
      next: (state) => {
        this.game = state;
        this.busy = false;
      },
      error: (err) => this.handleError(err)
    });
  }

  onNewGame(): void {
    this.game = null;
    this.errorMessage = null;
  }

  private handleError(err: HttpErrorResponse): void {
    this.busy = false;
    this.errorMessage = err.error?.message ?? 'Something went wrong talking to the server.';
  }
}

import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GameMode, GameStatus, PlayerSymbol } from '../../models/game.models';

@Component({
  selector: 'app-status-banner',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './status-banner.component.html',
  styleUrl: './status-banner.component.scss'
})
export class StatusBannerComponent {
  @Input({ required: true }) status!: GameStatus;
  @Input({ required: true }) currentPlayer!: PlayerSymbol;
  @Input() winner: PlayerSymbol | null = null;
  @Input({ required: true }) mode!: GameMode;

  get message(): string {
    if (this.status === 'Won') {
      return this.winner === 'O' && this.mode === 'VsComputer'
        ? 'The computer wins'
        : `${this.winner} wins`;
    }
    if (this.status === 'Draw') {
      return "It's a draw";
    }
    if (this.mode === 'VsComputer' && this.currentPlayer === 'O') {
      return 'Computer is thinking…';
    }
    return `${this.currentPlayer}'s turn`;
  }
}

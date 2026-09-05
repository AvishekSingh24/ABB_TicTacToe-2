import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PlayerSymbol } from '../../models/game.models';

@Component({
  selector: 'app-board',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './board.component.html',
  styleUrl: './board.component.scss'
})
export class BoardComponent {
  @Input({ required: true }) cells: (PlayerSymbol | null)[] = [];
  @Input() winningCells: number[] | null = null;
  @Input() disabled = false;

  @Output() cellChosen = new EventEmitter<number>();

  onCellClick(index: number): void {
    if (this.disabled || this.cells[index] !== null) {
      return;
    }
    this.cellChosen.emit(index);
  }

  isWinningCell(index: number): boolean {
    return this.winningCells?.includes(index) ?? false;
  }
}

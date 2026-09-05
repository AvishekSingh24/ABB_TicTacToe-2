import { Component, EventEmitter, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GameMode } from '../../models/game.models';

@Component({
  selector: 'app-mode-selector',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './mode-selector.component.html',
  styleUrl: './mode-selector.component.scss'
})
export class ModeSelectorComponent {
  @Output() modeChosen = new EventEmitter<GameMode>();

  choose(mode: GameMode): void {
    this.modeChosen.emit(mode);
  }
}

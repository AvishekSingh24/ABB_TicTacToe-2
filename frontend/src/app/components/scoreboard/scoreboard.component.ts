import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Scoreboard } from '../../models/game.models';

@Component({
  selector: 'app-scoreboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './scoreboard.component.html',
  styleUrl: './scoreboard.component.scss'
})
export class ScoreboardComponent {
  @Input({ required: true }) scoreboard!: Scoreboard;
  @Input() secondPlayerLabel = 'O';
}

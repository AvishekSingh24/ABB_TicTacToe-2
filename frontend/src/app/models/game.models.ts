export type PlayerSymbol = 'X' | 'O';
export type GameMode = 'TwoPlayer' | 'VsComputer';
export type GameStatus = 'InProgress' | 'Won' | 'Draw';

export interface MoveHistoryItem {
  moveNumber: number;
  player: PlayerSymbol;
  cellIndex: number;
  row: number;
  column: number;
  wasComputerMove: boolean;
}

export interface Scoreboard {
  xWins: number;
  oWins: number;
  draws: number;
}

export interface GameStateResponse {
  gameId: string;
  mode: GameMode;
  board: (PlayerSymbol | null)[];
  currentPlayer: PlayerSymbol;
  status: GameStatus;
  winner: PlayerSymbol | null;
  winningCells: number[] | null;
  moveHistory: MoveHistoryItem[];
  canUndo: boolean;
  scoreboard: Scoreboard;
}

export interface ApiError {
  message: string;
}

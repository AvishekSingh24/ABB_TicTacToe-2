import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { GameMode, GameStateResponse, PlayerSymbol, Scoreboard } from '../models/game.models';

@Injectable({ providedIn: 'root' })
export class GameService {
  private readonly baseUrl = environment.apiBaseUrl;

  constructor(private readonly http: HttpClient) {}

  createGame(mode: GameMode): Observable<GameStateResponse> {
    return this.http.post<GameStateResponse>(`${this.baseUrl}/games`, { mode });
  }

  getGame(gameId: string): Observable<GameStateResponse> {
    return this.http.get<GameStateResponse>(`${this.baseUrl}/games/${gameId}`);
  }

  makeMove(gameId: string, player: PlayerSymbol, cellIndex: number): Observable<GameStateResponse> {
    return this.http.post<GameStateResponse>(`${this.baseUrl}/games/${gameId}/moves`, {
      player,
      cellIndex
    });
  }

  undo(gameId: string): Observable<GameStateResponse> {
    return this.http.post<GameStateResponse>(`${this.baseUrl}/games/${gameId}/undo`, {});
  }

  resetGame(gameId: string): Observable<GameStateResponse> {
    return this.http.post<GameStateResponse>(`${this.baseUrl}/games/${gameId}/reset`, {});
  }

  getScoreboard(): Observable<Scoreboard> {
    return this.http.get<Scoreboard>(`${this.baseUrl}/scoreboard`);
  }

  resetScoreboard(): Observable<Scoreboard> {
    return this.http.post<Scoreboard>(`${this.baseUrl}/scoreboard/reset`, {});
  }
}

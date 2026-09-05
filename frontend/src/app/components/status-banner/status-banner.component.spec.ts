import { ComponentFixture, TestBed } from '@angular/core/testing';
import { StatusBannerComponent } from './status-banner.component';

describe('StatusBannerComponent', () => {
  let fixture: ComponentFixture<StatusBannerComponent>;
  let component: StatusBannerComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StatusBannerComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(StatusBannerComponent);
    component = fixture.componentInstance;
  });

  it('shows whose turn it is when in progress', () => {
    component.status = 'InProgress';
    component.currentPlayer = 'X';
    component.mode = 'TwoPlayer';

    expect(component.message).toBe("X's turn");
  });

  it('shows a thinking message while the computer is up', () => {
    component.status = 'InProgress';
    component.currentPlayer = 'O';
    component.mode = 'VsComputer';

    expect(component.message).toBe('Computer is thinking…');
  });

  it('announces the winner', () => {
    component.status = 'Won';
    component.currentPlayer = 'O';
    component.winner = 'X';
    component.mode = 'TwoPlayer';

    expect(component.message).toBe('X wins');
  });

  it('announces a computer win distinctly in vs-computer mode', () => {
    component.status = 'Won';
    component.currentPlayer = 'X';
    component.winner = 'O';
    component.mode = 'VsComputer';

    expect(component.message).toBe('The computer wins');
  });

  it('announces a draw', () => {
    component.status = 'Draw';
    component.currentPlayer = 'X';
    component.mode = 'TwoPlayer';

    expect(component.message).toBe("It's a draw");
  });
});

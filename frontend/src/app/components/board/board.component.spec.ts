import { ComponentFixture, TestBed } from '@angular/core/testing';
import { BoardComponent } from './board.component';

describe('BoardComponent', () => {
  let fixture: ComponentFixture<BoardComponent>;
  let component: BoardComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BoardComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(BoardComponent);
    component = fixture.componentInstance;
    component.cells = [null, null, null, null, null, null, null, null, null];
    fixture.detectChanges();
  });

  it('emits cellChosen when an empty cell is clicked', () => {
    const emitted: number[] = [];
    component.cellChosen.subscribe((i) => emitted.push(i));

    component.onCellClick(4);

    expect(emitted).toEqual([4]);
  });

  it('does not emit when the cell is already occupied', () => {
    component.cells = ['X', null, null, null, null, null, null, null, null];
    const emitted: number[] = [];
    component.cellChosen.subscribe((i) => emitted.push(i));

    component.onCellClick(0);

    expect(emitted).toEqual([]);
  });

  it('does not emit when the board is disabled', () => {
    component.disabled = true;
    const emitted: number[] = [];
    component.cellChosen.subscribe((i) => emitted.push(i));

    component.onCellClick(2);

    expect(emitted).toEqual([]);
  });

  it('reports winning cells correctly', () => {
    component.winningCells = [0, 1, 2];

    expect(component.isWinningCell(1)).toBeTrue();
    expect(component.isWinningCell(5)).toBeFalse();
  });
});

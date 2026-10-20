using System.Drawing;
using System.Windows.Forms;

namespace EightQueenGUI;

public sealed class MainForm : Form
{
    private const int BoardSize = 8;
    private readonly Button[,] _squares = new Button[BoardSize, BoardSize];
    private readonly List<int[]> _solutions = new();
    private readonly Label _statusLabel;
    private readonly Label _countLabel;
    private readonly Button _solveButton;
    private readonly Button _nextButton;
    private readonly Button _resetButton;
    private int _solutionIndex = -1;
    private readonly int[] _manualQueens = Enumerable.Repeat(-1, BoardSize).ToArray();

    private static readonly Color LightSquare = Color.FromArgb(238, 238, 210);
    private static readonly Color DarkSquare = Color.FromArgb(118, 150, 86);
    private static readonly Color QueenColor = Color.FromArgb(38, 45, 42);
    private static readonly Color AccentColor = Color.FromArgb(55, 105, 75);

    public MainForm()
    {
        Text = "8 Queens Puzzle";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(680, 650);
        ClientSize = new Size(760, 760);
        BackColor = Color.FromArgb(247, 248, 245);
        Font = new Font("Segoe UI", 10F);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(22),
            BackColor = BackColor
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        Controls.Add(root);

        var heading = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = BackColor
        };
        heading.RowStyles.Add(new RowStyle(SizeType.Percent, 62));
        heading.RowStyles.Add(new RowStyle(SizeType.Percent, 38));

        heading.Controls.Add(new Label
        {
            Text = "8 Queens Puzzle",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 22F, FontStyle.Bold),
            ForeColor = AccentColor,
            TextAlign = ContentAlignment.MiddleCenter
        }, 0, 0);
        heading.Controls.Add(new Label
        {
            Text = "Place eight queens so that no two queens attack each other.",
            Dock = DockStyle.Fill,
            ForeColor = Color.FromArgb(85, 90, 85),
            TextAlign = ContentAlignment.MiddleCenter
        }, 0, 1);
        root.Controls.Add(heading, 0, 0);

        var boardHost = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(4),
            BackColor = Color.FromArgb(45, 55, 45)
        };
        root.Controls.Add(boardHost, 0, 1);

        var board = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = BoardSize,
            ColumnCount = BoardSize,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            CellBorderStyle = TableLayoutPanelCellBorderStyle.None
        };
        for (int i = 0; i < BoardSize; i++)
        {
            board.RowStyles.Add(new RowStyle(SizeType.Percent, 12.5F));
            board.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12.5F));
        }

        for (int row = 0; row < BoardSize; row++)
        {
            for (int col = 0; col < BoardSize; col++)
            {
                var square = new Button
                {
                    Dock = DockStyle.Fill,
                    Margin = Padding.Empty,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = (row + col) % 2 == 0 ? LightSquare : DarkSquare,
                    ForeColor = QueenColor,
                    Font = new Font("Segoe UI Symbol", 30F, FontStyle.Bold),
                    Text = "",
                    TabStop = false,
                    AccessibleName = $"Row {row + 1}, column {col + 1}"
                };
                square.FlatAppearance.BorderSize = 0;
                int capturedRow = row;
                int capturedCol = col;
                square.Click += (_, _) => ToggleManualQueen(capturedRow, capturedCol);
                _squares[row, col] = square;
                board.Controls.Add(square, col, row);
            }
        }
        boardHost.Controls.Add(board);

        var controls = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 12, 0, 0),
            BackColor = BackColor
        };
        root.Controls.Add(controls, 0, 2);

        _solveButton = CreateActionButton("Solve", AccentColor);
        _solveButton.Click += (_, _) => ShowFirstSolution();
        controls.Controls.Add(_solveButton);

        _nextButton = CreateActionButton("Next Solution", Color.FromArgb(75, 105, 145));
        _nextButton.Click += (_, _) => ShowNextSolution();
        controls.Controls.Add(_nextButton);

        _resetButton = CreateActionButton("Reset", Color.FromArgb(105, 110, 105));
        _resetButton.Click += (_, _) => ResetBoard();
        controls.Controls.Add(_resetButton);

        var runnerButton = CreateActionButton("Play Dino Runner", Color.FromArgb(70, 95, 135));
        runnerButton.Width = 170;
        runnerButton.Click += (_, _) =>
        {
            using var runner = new DinoRunnerForm();
            runner.ShowDialog(this);
        };
        controls.Controls.Add(runnerButton);

        var footer = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = BackColor
        };
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68));
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));
        root.Controls.Add(footer, 0, 3);

        _statusLabel = new Label
        {
            Text = "Click squares to place queens, or press Solve.",
            Dock = DockStyle.Fill,
            ForeColor = Color.FromArgb(65, 70, 65),
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true
        };
        footer.Controls.Add(_statusLabel, 0, 0);

        _countLabel = new Label
        {
            Text = "Solution: — / 92",
            Dock = DockStyle.Fill,
            ForeColor = AccentColor,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleRight
        };
        footer.Controls.Add(_countLabel, 1, 0);

        BuildSolutions();
        UpdateBoardFromManualQueens();
    }

    private static Button CreateActionButton(string text, Color color)
    {
        var button = new Button
        {
            Text = text,
            Width = text == "Next Solution" ? 150 : 115,
            Height = 44,
            Margin = new Padding(0, 0, 12, 0),
            BackColor = color,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        button.FlatAppearance.BorderSize = 0;
        return button;
    }

    private void BuildSolutions()
    {
        _solutions.Clear();
        var queens = new int[BoardSize];
        Array.Fill(queens, -1);
        FindSolutions(0, queens);
    }

    private void FindSolutions(int row, int[] queens)
    {
        if (row == BoardSize)
        {
            _solutions.Add((int[])queens.Clone());
            return;
        }

        for (int col = 0; col < BoardSize; col++)
        {
            bool safe = true;
            for (int previousRow = 0; previousRow < row; previousRow++)
            {
                if (queens[previousRow] == col ||
                    Math.Abs(queens[previousRow] - col) == Math.Abs(previousRow - row))
                {
                    safe = false;
                    break;
                }
            }

            if (!safe)
                continue;

            queens[row] = col;
            FindSolutions(row + 1, queens);
            queens[row] = -1;
        }
    }

    private void ShowFirstSolution()
    {
        if (_solutions.Count == 0)
        {
            _statusLabel.Text = "No solution found.";
            return;
        }

        _solutionIndex = 0;
        DisplaySolution(_solutions[_solutionIndex]);
        _statusLabel.Text = "Solution found. Use Next Solution to browse all valid boards.";
    }

    private void ShowNextSolution()
    {
        if (_solutions.Count == 0)
            BuildSolutions();

        _solutionIndex = (_solutionIndex + 1) % _solutions.Count;
        DisplaySolution(_solutions[_solutionIndex]);
        _statusLabel.Text = "Every queen is safe: no shared row, column, or diagonal.";
    }

    private void DisplaySolution(int[] solution)
    {
        Array.Copy(solution, _manualQueens, BoardSize);
        UpdateBoardFromManualQueens();
        _countLabel.Text = $"Solution: {_solutionIndex + 1} / {_solutions.Count}";
    }

    private void ToggleManualQueen(int row, int col)
    {
        if (_solutionIndex >= 0)
            _solutionIndex = -1;

        if (_manualQueens[row] == col)
        {
            _manualQueens[row] = -1;
            _statusLabel.Text = $"Removed queen from row {row + 1}.";
        }
        else
        {
            _manualQueens[row] = col;
            _statusLabel.Text = $"Placed queen at row {row + 1}, column {col + 1}.";
        }

        UpdateBoardFromManualQueens();
        _countLabel.Text = $"Queens: {_manualQueens.Count(c => c >= 0)} / 8";
    }

    private void UpdateBoardFromManualQueens()
    {
        for (int row = 0; row < BoardSize; row++)
        {
            for (int col = 0; col < BoardSize; col++)
            {
                bool hasQueen = _manualQueens[row] == col;
                _squares[row, col].Text = hasQueen ? "♛" : "";
                _squares[row, col].BackColor = (row + col) % 2 == 0 ? LightSquare : DarkSquare;
                _squares[row, col].ForeColor = QueenColor;
            }
        }
    }

    private void ResetBoard()
    {
        Array.Fill(_manualQueens, -1);
        _solutionIndex = -1;
        UpdateBoardFromManualQueens();
        _statusLabel.Text = "Board cleared. Click squares or press Solve.";
        _countLabel.Text = "Solution: — / 92";
    }
}

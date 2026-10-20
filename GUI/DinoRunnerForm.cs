using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace EightQueenGUI;

public sealed class DinoRunnerForm : Form
{
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 16 };
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private readonly Random _random = new();
    private readonly Label _scoreLabel = new();
    private readonly Button _restartButton = new();
    private readonly List<Rectangle> _obstacles = new();
    private float _playerX = 90;
    private float _playerY;
    private float _velocityY;
    private float _speed = 5;
    private int _score;
    private int _spawnCounter;
    private bool _jumping;
    private bool _gameOver;
    private bool _left;
    private bool _right;
    private const int GroundY = 350;
    private const int PlayerWidth = 38;
    private const int PlayerHeight = 42;

    public DinoRunnerForm()
    {
        Text = "Dino Runner";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(900, 500);
        MinimumSize = new Size(700, 420);
        BackColor = Color.White;
        DoubleBuffered = true;
        KeyPreview = true;

        _scoreLabel.AutoSize = true;
        _scoreLabel.Font = new Font("Consolas", 16, FontStyle.Bold);
        _scoreLabel.Location = new Point(20, 18);
        _scoreLabel.Text = "SCORE 000000";
        Controls.Add(_scoreLabel);

        _restartButton.Text = "Restart";
        _restartButton.Size = new Size(100, 36);
        _restartButton.Location = new Point(20, 55);
        _restartButton.Click += (_, _) => RestartGame();
        Controls.Add(_restartButton);

        KeyDown += HandleKeyDown;
        KeyUp += HandleKeyUp;
        Paint += DrawScene;
        _timer.Tick += UpdateGame;
        RestartGame();
        _timer.Start();
    }

    private void HandleKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode is Keys.Space or Keys.Up or Keys.W)
        {
            if (_gameOver) RestartGame();
            else if (!_jumping) { _velocityY = -12.5f; _jumping = true; }
            e.Handled = true;
        }
        if (e.KeyCode is Keys.Left or Keys.A) _left = true;
        if (e.KeyCode is Keys.Right or Keys.D) _right = true;
        if (e.KeyCode == Keys.R) RestartGame();
    }

    private void HandleKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode is Keys.Left or Keys.A) _left = false;
        if (e.KeyCode is Keys.Right or Keys.D) _right = false;
    }

    private void UpdateGame(object? sender, EventArgs e)
    {
        if (_gameOver) return;

        if (_left) _playerX = Math.Max(0, _playerX - 4.5f);
        if (_right) _playerX = Math.Min(ClientSize.Width - PlayerWidth, _playerX + 4.5f);

        if (_jumping)
        {
            _playerY += _velocityY;
            _velocityY += 0.58f;
            if (_playerY >= 0) { _playerY = 0; _velocityY = 0; _jumping = false; }
        }

        _score++;
        _speed = Math.Min(12, 5 + _score / 900f);
        _spawnCounter++;
        if (_spawnCounter >= _random.Next(65, 105))
        {
            int height = _random.Next(32, 57);
            _obstacles.Add(new Rectangle(ClientSize.Width + 10, GroundY - height, _random.Next(22, 34), height));
            _spawnCounter = 0;
        }

        for (int i = _obstacles.Count - 1; i >= 0; i--)
        {
            var obstacle = _obstacles[i];
            obstacle.X -= (int)_speed;
            _obstacles[i] = obstacle;
            if (obstacle.Right < 0) _obstacles.RemoveAt(i);
        }

        var player = new Rectangle((int)_playerX, GroundY - PlayerHeight + (int)_playerY, PlayerWidth, PlayerHeight);
        if (_obstacles.Any(o => player.IntersectsWith(o)))
        {
            _gameOver = true;
            _timer.Stop();
        }

        _scoreLabel.Text = $"SCORE {_score / 5:000000}";
        Invalidate();
    }

    private void DrawScene(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Color.White);
        using var groundPen = new Pen(Color.FromArgb(70, 70, 70), 2);
        using var dinoBrush = new SolidBrush(Color.FromArgb(65, 65, 65));
        using var cactusBrush = new SolidBrush(Color.FromArgb(45, 115, 75));
        using var cloudPen = new Pen(Color.Gainsboro, 2);
        int ground = Math.Min(GroundY, ClientSize.Height - 70);
        g.DrawLine(groundPen, 0, ground, ClientSize.Width, ground);

        // Clouds and distant ground details give the scene a simple runner-game look.
        g.DrawArc(cloudPen, ClientSize.Width / 2, 90, 55, 25, 180, 180);
        g.DrawArc(cloudPen, ClientSize.Width / 2 + 20, 82, 55, 33, 180, 180);

        float playerTop = ground - PlayerHeight + _playerY;
        g.FillRectangle(dinoBrush, _playerX + 4, playerTop + 10, 25, 26);
        g.FillRectangle(dinoBrush, _playerX + 20, playerTop, 18, 19);
        g.FillRectangle(Brushes.White, _playerX + 31, playerTop + 4, 3, 3);
        g.FillRectangle(dinoBrush, _playerX + 7, playerTop + 33, 8, 9);
        g.FillRectangle(dinoBrush, _playerX + 23, playerTop + 33, 8, 9);

        foreach (var obstacle in _obstacles)
        {
            g.FillRectangle(cactusBrush, obstacle);
            g.FillRectangle(cactusBrush, obstacle.X - 7, obstacle.Y + 12, 9, 5);
            g.FillRectangle(cactusBrush, obstacle.X + obstacle.Width - 2, obstacle.Y + 22, 8, 5);
        }

        if (_gameOver)
        {
            using var overlayFont = new Font("Segoe UI", 28, FontStyle.Bold);
            using var hintFont = new Font("Segoe UI", 13, FontStyle.Regular);
            var message = "GAME OVER";
            var size = g.MeasureString(message, overlayFont);
            g.DrawString(message, overlayFont, Brushes.Firebrick, (ClientSize.Width - size.Width) / 2, 145);
            var hint = "Press Space or R to restart";
            var hintSize = g.MeasureString(hint, hintFont);
            g.DrawString(hint, hintFont, Brushes.DimGray, (ClientSize.Width - hintSize.Width) / 2, 190);
        }
        else
        {
            using var helpFont = new Font("Segoe UI", 10);
            g.DrawString("Move: ← → / A D     Jump: Space / ↑ / W", helpFont, Brushes.DimGray, 20, 105);
        }
    }

    private void RestartGame()
    {
        _playerX = 90;
        _playerY = 0;
        _velocityY = 0;
        _jumping = false;
        _gameOver = false;
        _score = 0;
        _spawnCounter = 0;
        _speed = 5;
        _obstacles.Clear();
        _scoreLabel.Text = "SCORE 000000";
        _clock.Restart();
        if (!_timer.Enabled) _timer.Start();
        Invalidate();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _timer.Stop();
        _timer.Dispose();
        base.OnFormClosed(e);
    }
}

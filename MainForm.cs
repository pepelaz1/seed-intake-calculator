using System.Drawing.Drawing2D;

namespace SeedIntakeCalculator;

public sealed class MainForm : Form
{
    private static readonly Color Ink = Color.FromArgb(28, 43, 43);
    private static readonly Color Muted = Color.FromArgb(101, 117, 115);
    private static readonly Color Accent = Color.FromArgb(20, 111, 96);
    private static readonly Color Surface = Color.FromArgb(247, 249, 247);
    private readonly NumericUpDown weightInput;
    private readonly NumericUpDown moistureInput;
    private readonly Label acceptedValue;
    private readonly Label discountValue;
    private readonly Label errorLabel;
    private readonly Panel breakdownRows;

    public MainForm()
    {
        Text = "Приёмка семян · расчёт зачётного веса";
        AutoScaleMode = AutoScaleMode.None;
        ClientSize = new Size(960, 590);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Surface;
        ForeColor = Ink;
        Font = new Font("Segoe UI", 10);

        var header = new Panel { Bounds = new Rectangle(0, 0, 960, 88), BackColor = Color.White };
        Controls.Add(header);
        header.Controls.Add(TextLabel("SEED / INTAKE", 24, 11, 250, 20, 9, Accent, FontStyle.Bold));
        header.Controls.Add(TextLabel("Приёмка семян", 24, 35, 530, 38, 21, Ink, FontStyle.Bold));
        header.Controls.Add(TextLabel("Демонстрационный расчёт по формуле заказчика", 588, 42, 348, 24, 10, Muted, FontStyle.Regular, ContentAlignment.MiddleRight));

        var inputHeading = TextLabel("Исходные данные", 24, 100, 300, 31, 15, Ink, FontStyle.Bold);
        Controls.Add(inputHeading);

        var weightCard = new Card { Bounds = new Rectangle(24, 140, 300, 118) };
        Controls.Add(weightCard);
        weightCard.Controls.Add(TextLabel("Физический вес", 20, 10, 240, 25, 11, Ink, FontStyle.Bold));
        weightCard.Controls.Add(TextLabel("Вес поступившей партии, кг", 20, 35, 250, 20, 9, Muted));
        weightInput = NumberInput(20, 67, 215, 100m, 100000000m);
        weightCard.Controls.Add(weightInput);
        weightCard.Controls.Add(TextLabel("кг", 241, 72, 38, 28, 10, Muted));

        var moistureCard = new Card { Bounds = new Rectangle(24, 270, 300, 118) };
        Controls.Add(moistureCard);
        moistureCard.Controls.Add(TextLabel("Влажность", 20, 10, 240, 25, 11, Ink, FontStyle.Bold));
        moistureCard.Controls.Add(TextLabel("Лабораторный показатель, %", 20, 35, 255, 20, 9, Muted));
        moistureInput = NumberInput(20, 67, 215, 20m, 100m);
        moistureInput.Minimum = 4m;
        moistureCard.Controls.Add(moistureInput);
        moistureCard.Controls.Add(TextLabel("%", 241, 72, 38, 28, 10, Muted));

        var pendingCard = new Card { Bounds = new Rectangle(24, 400, 300, 130) };
        Controls.Add(pendingCard);
        pendingCard.Controls.Add(TextLabel("Остальные показатели", 20, 10, 260, 25, 11, Ink, FontStyle.Bold));
        pendingCard.Controls.Add(TextLabel("4 из 5 правил ещё не переданы.", 20, 41, 260, 21, 9, Muted));
        pendingCard.Controls.Add(TextLabel("После получения диапазонов и ставок\nих можно добавить в расчёт.", 20, 67, 260, 48, 9, Muted));

        Controls.Add(TextLabel("Результат", 350, 100, 300, 31, 15, Ink, FontStyle.Bold));
        var resultCard = new Card { Bounds = new Rectangle(350, 140, 586, 150) };
        Controls.Add(resultCard);
        resultCard.Controls.Add(TextLabel("ЗАЧЁТНЫЙ ВЕС", 24, 12, 370, 22, 9, Accent, FontStyle.Bold));
        acceptedValue = TextLabel("81,500 кг", 24, 33, 380, 60, 31, Ink, FontStyle.Bold);
        resultCard.Controls.Add(acceptedValue);
        discountValue = TextLabel("Скидка по влажности 18,5 %", 24, 103, 550, 28, 11, Muted);
        resultCard.Controls.Add(discountValue);

        var breakdownCard = new Card { Bounds = new Rectangle(350, 303, 586, 227) };
        Controls.Add(breakdownCard);
        breakdownCard.Controls.Add(TextLabel("Расшифровка скидки", 24, 11, 500, 31, 13, Ink, FontStyle.Bold));
        breakdownRows = new Panel { Bounds = new Rectangle(24, 42, 538, 176), BackColor = Color.White };
        breakdownCard.Controls.Add(breakdownRows);

        errorLabel = TextLabel("", 24, 543, 912, 30, 9, Color.FromArgb(174, 65, 47));
        Controls.Add(errorLabel);
        weightInput.ValueChanged += (_, _) => Recalculate();
        moistureInput.ValueChanged += (_, _) => Recalculate();
        Recalculate();
    }

    private void Recalculate()
    {
        try
        {
            var discount = MoistureCalculator.CalculateDiscount(moistureInput.Value);
            var accepted = MoistureCalculator.CalculateAcceptedWeight(weightInput.Value, discount);
            acceptedValue.Text = $"{accepted:N3} кг";
            discountValue.Text = $"Скидка по влажности {discount:0.##} % · удержано {weightInput.Value - accepted:N3} кг";
            errorLabel.Text = "Расчёт демонстрирует только скидку по влажности. Итог для всех 5 показателей требует полного ТЗ.";
            RenderBreakdown(moistureInput.Value, discount);
        }
        catch (ArgumentOutOfRangeException)
        {
            acceptedValue.Text = "—";
            discountValue.Text = "Расчёт невозможен";
            errorLabel.Text = "Суммарная скидка превысила 100 %. Проверьте лабораторный показатель.";
            breakdownRows.Controls.Clear();
        }
    }

    private void RenderBreakdown(decimal moisture, decimal total)
    {
        breakdownRows.SuspendLayout();
        breakdownRows.Controls.Clear();
        var y = 0;
        foreach (var (range, amount) in MoistureCalculator.Breakdown(moisture))
        {
            breakdownRows.Controls.Add(TextLabel(range, 0, y, 385, 30, 10, amount == 0m ? Muted : Ink));
            breakdownRows.Controls.Add(TextLabel($"{amount:0.##} %", 398, y, 140, 30, 10, amount == 0m ? Muted : Ink, FontStyle.Bold, ContentAlignment.MiddleRight));
            y += 28;
        }
        var line = new Panel { Bounds = new Rectangle(0, 139, 538, 1), BackColor = Color.FromArgb(222, 230, 226) };
        breakdownRows.Controls.Add(line);
        breakdownRows.Controls.Add(TextLabel("Итого по влажности", 0, 143, 385, 25, 10, Ink, FontStyle.Bold));
        breakdownRows.Controls.Add(TextLabel($"{total:0.##} %", 398, 143, 140, 25, 10, Accent, FontStyle.Bold, ContentAlignment.MiddleRight));
        breakdownRows.ResumeLayout();
    }

    private static Label TextLabel(string text, int x, int y, int width, int height, float size, Color color,
        FontStyle style = FontStyle.Regular, ContentAlignment align = ContentAlignment.MiddleLeft) =>
        new()
        {
            Text = text, Bounds = new Rectangle(x, y, width, height),
            Font = new Font("Segoe UI", size, style), ForeColor = color,
            TextAlign = align, BackColor = Color.Transparent
        };

    private static NumericUpDown NumberInput(int x, int y, int width, decimal initial, decimal max) =>
        new()
        {
            Bounds = new Rectangle(x, y, width, 35), Font = new Font("Segoe UI", 12, FontStyle.Bold),
            DecimalPlaces = 2, ThousandsSeparator = true, Increment = 0.1m,
            Minimum = 0m, Maximum = max, Value = initial, BackColor = Color.White,
            ForeColor = Ink, BorderStyle = BorderStyle.FixedSingle
        };

    private sealed class Card : Panel
    {
        public Card() => BackColor = Color.White;

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var pen = new Pen(Color.FromArgb(222, 230, 226));
            using var path = new GraphicsPath();
            var r = new Rectangle(0, 0, Width - 1, Height - 1);
            const int d = 17;
            path.AddArc(r.Left, r.Top, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Top, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.Left, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            e.Graphics.DrawPath(pen, path);
        }
    }
}

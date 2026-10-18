using System.Globalization;
using System.Drawing.Imaging;

namespace SeedIntakeCalculator;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Contains("--self-test"))
        {
            (decimal Moisture, decimal Expected)[] examples =
            [
                (4m, 5m), (5m, 2.5m), (6m, 0m), (8m, 0m),
                (12m, 4m), (15m, 8.5m), (20m, 18.5m), (21m, 21.5m)
            ];
            foreach (var (moisture, expected) in examples)
            {
                var actual = MoistureCalculator.CalculateDiscount(moisture);
                if (actual != expected)
                    throw new Exception($"{moisture}%: expected {expected}, got {actual}");
            }
            if (MoistureCalculator.CalculateAcceptedWeight(100m, 18.5m) != 81.5m)
                throw new Exception("Example weight did not match.");
            Console.WriteLine("8 moisture boundaries and accepted-weight example passed.");
            return 0;
        }

        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo("ru-RU");
        ApplicationConfiguration.Initialize();
        if (args.Length == 2 && args[0] == "--screenshot")
        {
            using var form = new MainForm();
            form.Show();
            Application.DoEvents();
            using var bitmap = new Bitmap(form.Width, form.Height);
            form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, form.Size));
            bitmap.Save(args[1], ImageFormat.Png);
            form.Close();
            Console.WriteLine(args[1]);
            return 0;
        }
        Application.Run(new MainForm());
        return 0;
    }
}

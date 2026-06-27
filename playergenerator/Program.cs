using System.Globalization;
using System.Text.Json;
using System.Windows.Forms;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        using OpenFileDialog dlg = new();

        dlg.Title = "Выберите CSV файл";
        dlg.Filter = "CSV files (*.csv)|*.csv";

        if (dlg.ShowDialog() != DialogResult.OK)
            return;

        string csvFile = dlg.FileName;

        try
        {
            GeneratePlayersJson(csvFile);

            MessageBox.Show(
                "Файл players.json успешно создан.",
                "Готово",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Ошибка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    static void GeneratePlayersJson(string csvFile)
    {
        string[] lines = File.ReadAllLines(csvFile);

        List<(int Id, double Points)> sourcePlayers = new();

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] parts = line.Split(';');

            if (parts.Length < 2)
                continue;

            // Если первая колонка не число,
            // значит это заголовок или мусорная строка
            if (!int.TryParse(parts[0].Trim(), out int id))
                continue;

            string pointsText = parts[1]
                .Trim()
                .Replace(',', '.');

            if (!double.TryParse(
                    pointsText,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out double points))
            {
                continue;
            }

            sourcePlayers.Add((id, points));
        }

        Dictionary<int, double> multipliers =
            sourcePlayers.ToDictionary(x => x.Id, x => 1.0);

        var ranked = sourcePlayers
            .Where(x => x.Points > 0)
            .OrderByDescending(x => x.Points)
            .ToList();

        for (int i = 0; i < ranked.Count; i++)
        {
            int place = i + 1;

            double multiplier =
                place <= 3 ? 1.5 :
                place <= 10 ? 1.4 :
                place <= 20 ? 1.3 :
                place <= 30 ? 1.2 :
                1.1;

            multipliers[ranked[i].Id] = multiplier;
        }

        var players = sourcePlayers
            .Select(x => new
            {
                id = x.Id,
                multiplier = multipliers[x.Id]
            })
            .ToList();

        var result = new
        {
            players
        };

        string json = JsonSerializer.Serialize(
            result,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        string outputFile = Path.Combine(
                 AppDomain.CurrentDomain.BaseDirectory,
                "players.json");

        File.WriteAllText(outputFile, json);
    }
}
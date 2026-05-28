using System.IO;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage.Streams;

namespace Freezshot.Capture;

/// <summary>
/// Thin wrapper around Windows.Media.Ocr (built into Windows 10+). No network,
/// no API key — uses whatever OCR language packs the user has installed.
/// </summary>
internal static class OcrHelper
{
    public static async Task<string?> ExtractAsync(byte[] pngBytes)
    {
        try
        {
            using var stream = new InMemoryRandomAccessStream();
            using (var writer = new DataWriter(stream))
            {
                writer.WriteBytes(pngBytes);
                await writer.StoreAsync();
                await writer.FlushAsync();
                writer.DetachStream();
            }
            stream.Seek(0);
            var decoder = await BitmapDecoder.CreateAsync(stream);
            using var bitmap = await decoder.GetSoftwareBitmapAsync();

            var engine = OcrEngine.TryCreateFromUserProfileLanguages() ?? OcrEngine.TryCreateFromLanguage(new Windows.Globalization.Language("en"));
            if (engine is null) return null;

            var result = await engine.RecognizeAsync(bitmap);
            return string.IsNullOrWhiteSpace(result.Text) ? null : result.Text.Trim();
        }
        catch
        {
            return null;
        }
    }
}

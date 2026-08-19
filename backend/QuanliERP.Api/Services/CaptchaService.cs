using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace QuanliERP.Api.Services
{
    public class CaptchaService
    {
        private readonly ConcurrentDictionary<string, (string Code, DateTime Expire)> _store = new();

        public (string Key, string ImageBase64) Generate()
        {
            var code = GenerateCode();
            var key = Guid.NewGuid().ToString("N");
            _store[key] = (code, DateTime.Now.AddMinutes(5));

            var img = DrawImage(code);
            using var ms = new MemoryStream();
            img.Save(ms, ImageFormat.Png);
            img.Dispose();
            var base64 = "data:image/png;base64," + Convert.ToBase64String(ms.ToArray());

            Cleanup();
            return (key, base64);
        }

        public bool Validate(string key, string code)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(code)) return false;
            if (!_store.TryRemove(key, out var item)) return false;
            return item.Expire > DateTime.Now && string.Equals(item.Code, code, StringComparison.OrdinalIgnoreCase);
        }

        private static string GenerateCode()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var rnd = Random.Shared;
            return new string(Enumerable.Range(0, 4).Select(_ => chars[rnd.Next(chars.Length)]).ToArray());
        }

        private static Bitmap DrawImage(string code)
        {
            var w = 120; var h = 40;
            var bmp = new Bitmap(w, h);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.White);

            var rnd = Random.Shared;
            var colors = new[] { Color.FromArgb(150, 0, 0, 0), Color.FromArgb(120, 0, 0, 0), Color.FromArgb(100, 0, 0, 0) };

            for (var i = 0; i < 4; i++)
                g.DrawLine(new Pen(colors[rnd.Next(colors.Length)], 1), rnd.Next(0, w), rnd.Next(0, h), rnd.Next(0, w), rnd.Next(0, h));

            for (var i = 0; i < 50; i++)
                bmp.SetPixel(rnd.Next(w), rnd.Next(h), Color.FromArgb(rnd.Next(150, 230), rnd.Next(200), rnd.Next(200), rnd.Next(200)));

            using var font = new Font("Consolas", 22, FontStyle.Bold);
            for (var i = 0; i < code.Length; i++)
            {
                g.DrawString(code[i].ToString(), font, new SolidBrush(Color.FromArgb(rnd.Next(50, 180), rnd.Next(100, 255), rnd.Next(100, 255), rnd.Next(100, 255))), i * 26 + 5, rnd.Next(2, 10));
            }

            return bmp;
        }

        private void Cleanup()
        {
            var now = DateTime.Now;
            foreach (var kv in _store.Where(kv => kv.Value.Expire < now).ToList())
                _store.TryRemove(kv.Key, out _);
        }
    }
}

using SharpVectors.Converters;
using SharpVectors.Renderers.Wpf;
using System.Windows.Controls;
using System.Windows.Media;


namespace SaveDataWpf.Helper
{
    internal static class IconHelper
    {
        public static Image CopyBtnIcon => LoadIcons(FileSystemHelper.GetDynamicPath(@"Icons\Copy.svg"));
        public static Image EncryptBtnIcon => LoadIcons(FileSystemHelper.GetDynamicPath(@"Icons\Encrypt.svg"));
        public static Image DecryptBtnIcon => LoadIcons(FileSystemHelper.GetDynamicPath(@"Icons\Decrypt.svg"));
        public static Image EditBtnIcon => LoadIcons(FileSystemHelper.GetDynamicPath(@"Icons\Edit.svg"));
        public static Image DeleteBtnIcon => LoadIcons(FileSystemHelper.GetDynamicPath(@"Icons\Delete.svg"));

        private static Image LoadIcons(string pathToSvg)
        {
            WpfDrawingSettings settings = new()
            {
                IncludeRuntime = false,
                TextAsGeometry = true
            };

            FileSvgReader reader = new(settings);
            DrawingGroup cache = reader.Read(pathToSvg);

            DrawingImage drawing = new(cache);
            Image image = new()
            {
                Source = drawing,
                Width = 24,
                Height = 24
            };

            return image;
        }
    }
}

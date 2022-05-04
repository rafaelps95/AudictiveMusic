using ClassLibrary.Helpers;

namespace ClassLibrary.Entities
{
    public class FolderItem : MediaItem
    {
        public string Path { get; set; }

        public bool IsFolder { get; set; }

        public string Glyph
        {
            get
            {
                if (IsFolder)
                    return "";
                else if (StorageHelper.IsMusicFile(Path))
                    return "";
                else
                    return "";
            }
        }

        public FolderItem(string name, string path, bool isFolder)
        {
            Name = name;
            Path = path;
            IsFolder = isFolder;
        }
    }
}

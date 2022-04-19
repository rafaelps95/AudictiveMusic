using ClassLibrary.Entities;
using ClassLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ClassLibrary.Control
{
    public static class Ctr_FolderItem
    {
        public static async Task<List<string>> GetSongs(FolderItem folderItem)
        {
            List<string> list = new List<string>();
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(folderItem.Path);
            var files = await StorageHelper.ScanFolder(folder);

            foreach (StorageFile f in files)
            {
                if (StorageHelper.IsMusicFile(f.Path))
                    list.Add(f.Path);
            }

            return list;
        }
    }
}

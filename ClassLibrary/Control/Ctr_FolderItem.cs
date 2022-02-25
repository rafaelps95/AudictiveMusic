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
        public static async Task<List<string>> GetSongs(FolderItem folder)
        {
            List<string> list = new List<string>();
            StorageFolder subFolder = await StorageFolder.GetFolderFromPathAsync(folder.Path);
            var subFolderItems = await StorageHelper.ReadFolder(subFolder);

            foreach (StorageFile f in subFolderItems)
            {
                if (StorageHelper.IsMusicFile(f.Path))
                    list.Add(f.Path);
            }

            return list;
        }
    }
}

﻿using System.IO;
using System.Linq;
using System.Text;

namespace Concord
{
    public static class BibleBuilder
    {
        internal static string GetBlob(string blobfile)
        {
            var name = System.Reflection.Assembly.GetAssembly(typeof(BibleBuilder))
                                             .GetManifestResourceNames()
                                             .FirstOrDefault(x => x.Contains(blobfile));

            var stream = System.Reflection.Assembly.GetAssembly(typeof(BibleBuilder))
                .GetManifestResourceStream(name);

            var blob = "";
            using (StreamReader sr = new StreamReader(stream))
            {
                blob = sr.ReadToEnd();
            }

            return blob;
        }


        public static HardCopyAPI BuildESV()
        {
            return new HardCopyAPI(GetBlob("ESV.json"), GetBlob("ESV-Books.json"));
        }
        public static HardCopyAPI BuildNIV()
        {
            return new HardCopyAPI(GetBlob("NIV.json"), GetBlob("NIV-Books.json"));
        }


    }
}

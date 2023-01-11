
using System;
using System.IO;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Zip;
using System.Text;
using System.Collections.Generic;
using ETL.File.InputTypes;

namespace ETL.Zip
{

    public static class ZipUtil
    {

        /// <summary>
        /// Assuming .tar.gzcontains single file
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static Stream ReadTarGz(StreamData input, Encoding enc = null)
        {
            if (enc == null) enc = Encoding.UTF8;
            var gz = new GZipInputStream(input.Stream);
            var tar = new TarInputStream(gz, enc);
            var entry = tar.GetNextEntry();
            return tar;

        }


        public static Stream ReadTarGz(StreamData input, String fileName, Encoding enc = null)
        {

            if (enc == null) enc = Encoding.UTF8;

            var gz = new GZipInputStream(input.Stream);
            var tar = new TarInputStream(gz, enc);

            //var entry = tar.GetNextEntry();
            TarEntry e = tar.GetNextEntry();
            if(String.IsNullOrEmpty(fileName)) return tar;

            while (e != null )
            {
                if(e.Name == fileName) break;
                e = tar.GetNextEntry();
            }
            if (e == null) throw new Exception("this archive does not contain file: " + fileName);

            return tar;

        }

         // this could be useful in cmdlet for lazy reading
        internal static IEnumerable<String> ReadTarGzLines(StreamReader reader)
        {
            while (!reader.EndOfStream)
            {
                yield return reader.ReadLine();
            }

            reader.Dispose();
            reader.BaseStream.Dispose();

        }


        public static List<String> ReadTarGzLines(StreamData input, Encoding enc = null)
        {

            var list = new List<String>();

            if (enc == null) enc = Encoding.UTF8;

            var gz = new GZipInputStream(input.Stream);
            var tar = new TarInputStream(gz, enc);

            var entry = tar.GetNextEntry();

            var reader = new StreamReader(tar);

            while(!reader.EndOfStream) {
                list.Add(reader.ReadLine());
            }

            return list;

        }

        public static Stream ReadTar(StreamData input, Encoding enc = null)
        {

            if (enc == null) enc = Encoding.UTF8;
            return new TarInputStream(input.Stream, enc);

        }

        public static Stream ReadGzip(StreamData input)
        {

            return new GZipInputStream(input.Stream);

        }

        public static Stream ReadZip(StreamData input)
        {

            return new ZipInputStream(input.Stream);

        }

        public static Stream ReadZip(StreamData input, String fileName)
        {
            var zip = new ZipInputStream(input.Stream);
            var e = zip.GetNextEntry();

            if(String.IsNullOrEmpty(fileName)) return zip;

            while (e != null )
            {
                if(e.Name == fileName) break;
                e = zip.GetNextEntry();
            }
            if (e == null) throw new Exception("this archive does not contain file: " + fileName);

            return zip;         

        }

    }

}
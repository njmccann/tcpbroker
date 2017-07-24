using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPBroker
{
    public class Stitch
    {

        public Stitch(Stream s1, Stream s2)
        {
            Task.Run(() => forward(s1, s2));
            Task.Run(() => forward(s2, s1));
        }

        private void forward(Stream inStream, Stream outStream)
        {
            var buff = new byte[4096];

            while (true)
            {
                var len = inStream.Read(buff, 0, buff.Length);
                outStream.Write(buff, 0, len);
            }
        }

    }
}

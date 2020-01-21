﻿using System;
using System.Diagnostics;
using PlasticMetal.MobileSuit;

namespace SystemCall
{
    class Program
    {
        static void Main(string[] args)
        {
            for (;;)
            {
                var ms = new MsHost(new MsTest());
                ms.Run();
                var s = new[] {1};
                var a = s[1..];
                Console.WriteLine(a.Length);
                Console.Read();
                
                Console.Out.Flush();
            }

        }


    }
}

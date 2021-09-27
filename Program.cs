using System;
using TracerNS;
using System.Collections.Generic;
using System.Threading;

namespace SPP1
{
    public class Foo
    {
        private Bar _bar;
        private ITracer _tracer;

        internal Foo(ITracer tracer)
        {
            _tracer = tracer;
            _bar = new Bar(_tracer);
        }

        public void MyMethod()
        {
            _tracer.StartTrace();
            
            _bar.InnerMethod();            
            
            _tracer.StopTrace();
        }
    }

    public class Bar
    {
        private ITracer _tracer;

        internal Bar(ITracer tracer)
        {
            _tracer = tracer;
        }

        public void InnerMethod()
        {
            _tracer.StartTrace();

            long temp = 1;
            for (long i = 2; i < 10; i++)
            {
                temp *= i;
            }

            Thread.Sleep(10);     

            _tracer.StopTrace();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Tracer tracer = new Tracer();
            Foo foo = new Foo(tracer);
            foo.MyMethod();
            foo.MyMethod();

            var temp = tracer.GetTraceResult();


            Console.WriteLine("Hello World!");
        }
    }
}

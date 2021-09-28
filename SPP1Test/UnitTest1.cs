using Microsoft.VisualStudio.TestTools.UnitTesting;
using SPP1;

using TracerNS;
using System.Threading;

namespace SPP1Test
{
    [TestClass]
    public class UnitTest1
    {
        public Tracer tracer;
        int id;

        public void Setup()
        {
            tracer = new Tracer();
            id = Thread.CurrentThread.ManagedThreadId;
        }

        [TestMethod]
        public void TestSingleThread()
        {
            Setup();
            var bar = new Bar(tracer);
            bar.InnerMethod();
            var result = tracer.GetTraceResult();
            
            Assert.AreEqual(true, result.threadsInfo[id] != null);
        }

        [TestMethod]
        public void TestSingleMethodAndClass()
        {
            Setup();
            var bar = new Bar(tracer);
            bar.InnerMethod();
            var result = tracer.GetTraceResult();
            
            Assert.AreEqual(nameof(bar.InnerMethod), result.threadsInfo[id].methods[0].methodName);
            Assert.AreEqual(nameof(Bar), result.threadsInfo[id].methods[0].className);
        }

        [TestMethod]
        public void TestInnerMethod()
        {
            Setup();
            var foo = new Foo(tracer);
            foo.MyMethod();
            var result = tracer.GetTraceResult();

            Assert.AreEqual(nameof(Bar), result.threadsInfo[id].methods[0].methods[0].className);
            Assert.AreEqual(nameof(Bar.InnerMethod), result.threadsInfo[id].methods[0].methods[0].methodName);
        }

        [TestMethod]
        public void TestTwoThreads()
        {
            Setup();
            var bar = new Bar(tracer);
            bar.InnerMethod();
            var result = tracer.GetTraceResult();

            Assert.AreEqual(true, result.threadsInfo[id] != null);

            var bar2 = new Bar(tracer);
            Thread thread = new Thread(new ThreadStart(bar2.InnerMethod));
            int id2 = thread.ManagedThreadId;
            thread.Start();
            thread.Join();
            Assert.AreEqual(true, result.threadsInfo[id2] != null);
            Assert.AreEqual(2, result.threadsInfo.Count);
        }

        [TestMethod]
        public void TestTwoThreadsAndInnerMethods()
        {
            Setup();
            var foo = new Foo(tracer);
            foo.MyMethod();
            foo.MyMethod();
            var result = tracer.GetTraceResult();

            Assert.AreEqual(true, result.threadsInfo[id] != null);
            Assert.AreEqual(2, result.threadsInfo[id].methods.Count);
            Assert.AreEqual(true, result.threadsInfo[id].methods[0].methodName == result.threadsInfo[id].methods[1].methodName);

            var foo2 = new Foo(tracer);
            Thread thread = new Thread(new ThreadStart(foo.MyMethod));
            int id2 = thread.ManagedThreadId;
            thread.Start();
            thread.Join();
            Assert.AreEqual(true, result.threadsInfo[id2] != null);
            Assert.AreEqual(2, result.threadsInfo.Count);
            Assert.AreEqual(1, result.threadsInfo[id2].methods.Count);

            Assert.AreEqual(nameof(foo.MyMethod), result.threadsInfo[id2].methods[0].methodName);
        }
    }
}

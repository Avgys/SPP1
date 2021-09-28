using System;

namespace TracerNS
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Diagnostics;
    using System.Text.Json.Serialization;
    using System.Xml;
    using System.Xml.Serialization;


    public interface ITracer
    {
        void StartTrace();
    
        void StopTrace();

        TraceResult GetTraceResult();
    }

    [Serializable]
    public class MethodInfo
    {
        
        private Stopwatch stopwatch;
        [XmlAttribute]
        public long timeInterval { get; set; }

        [JsonInclude]
        public string className {  get; set; }
        [JsonInclude]
        public string methodName {  get; set; }

        [JsonIgnore]
        [XmlIgnore]
        public MethodInfo parentMethod { get; private set; }

        public List<MethodInfo> methods { get; private set; }

        public MethodInfo()
        {            
        }

        public MethodInfo(string _className, string _methodName, MethodInfo _parentMethod)
        {
            className = _className;
            methodName = _methodName;
            parentMethod = _parentMethod;
            stopwatch = new Stopwatch();
        }

        public void AddSubMethod(MethodInfo subMethod)
        {
            if(methods == null)
            {
                methods = new List<MethodInfo>();
            }
            methods.Add(subMethod);
        }

        internal void StartTimer()
        {
            stopwatch.Start();
        }

        internal void StopTimer()
        {
            stopwatch.Stop();
            timeInterval = stopwatch.ElapsedMilliseconds;
        }
    }

    [Serializable]
    public class ThreadInfo
    {
        
        private Stopwatch stopwatch;
        [XmlElement(ElementName = "time")]
        public long timeInterval { get; set; }

        [XmlElement(ElementName = "methods")]
        public List<MethodInfo> methods { get; private set; }
        
        public ThreadInfo()
        {
            methods = new List<MethodInfo>();
            stopwatch = new Stopwatch();
        }

        internal void StartTimer()
        {
            stopwatch.Start();
        }

        internal void StopTimer()
        {
            stopwatch.Stop();
            timeInterval = stopwatch.ElapsedMilliseconds;
        }

        internal void AddMethod(MethodInfo _method)
        {
            if (_method != null)
            {
                methods.Add(_method);
            }
        }
    }

    

    [Serializable]
    public class TraceResult
    {        
        public Dictionary<int, ThreadInfo> threadsInfo { get; private set; }

        
        internal TraceResult()
        {
            threadsInfo = new Dictionary<int, ThreadInfo>();
        }

        internal void AddThread(int id, ThreadInfo _thread)
        {
            if (!isThreadExist(id)) 
            {
                threadsInfo.Add(id, _thread);
            }
        }

        internal bool isThreadExist(int id)
        {
           return threadsInfo.ContainsKey(id);
        }

        internal ThreadInfo getThreadInfo(int id)
        {
            if (isThreadExist(id))
            {
                return threadsInfo[id];
            }
            else
            {
                return null;
            }
        }
    }

    public class Tracer : ITracer
    {
        static private object GetTracerLocker = 0;
        static private object AddThreadLocker = 0;
        private TraceResult traceResult;
        private int counter = -1;

        private MethodInfo currentMethod;

        StackTrace st;
        private static Tracer tracer = null;

        static public Tracer GetTracer()
        {
            if (tracer != null)
            {
                return tracer;
            }
            else
            {
                lock (GetTracerLocker)
                {
                    if (tracer != null)
                    {
                        return tracer;
                    }
                    else
                    {
                        return tracer = new Tracer();
                    }
                }
            }
        }

        public Tracer()
        {
            traceResult = new TraceResult();
        }

        public void StartTrace()
        {
            counter++;            

            st = new StackTrace(false);
            string typeName = st.GetFrame(1).GetMethod().DeclaringType.Name;
            string methodName = st.GetFrame(1).GetMethod().Name;
            MethodInfo mInfo = new MethodInfo(typeName, methodName, currentMethod);
            mInfo.StartTimer();
            if (currentMethod == null)
            {
                int id = Thread.CurrentThread.ManagedThreadId;
                ThreadInfo tInfo = traceResult.getThreadInfo(id);
                if (tInfo == null)
                {
                    tInfo = new ThreadInfo();
                    tInfo.StartTimer();
                    lock (AddThreadLocker) 
                    {
                        traceResult.AddThread(id, tInfo);
                    }
                }
                tInfo.StartTimer();

                tInfo.AddMethod(mInfo);
                currentMethod = mInfo;
            }
            else
            {
                currentMethod.AddSubMethod(mInfo);                
                currentMethod = mInfo;
            }
            
        }

        public void StopTrace()
        {
            counter--;            
            currentMethod.StopTimer();
            currentMethod = currentMethod.parentMethod;
            if (counter < 0 || currentMethod == null) 
            {
                int id = Thread.CurrentThread.ManagedThreadId;
                ThreadInfo tInfo = traceResult.getThreadInfo(id);
                tInfo.StopTimer();                
            }
        }

        public TraceResult GetTraceResult()
        {
            return traceResult;
        }
    }
}

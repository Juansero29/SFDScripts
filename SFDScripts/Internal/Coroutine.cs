///*
//* author: JakSparro98
//* description: Unofficial Coroutine class to help in creation of coroutines.
//*/
//using SFDGameScriptInterface;
//using System.Collections.Generic;

//public class Coroutine : GameScriptInterface
//{
//    static List<Coroutine> RoutinesList = new List<Coroutine>();
//    public bool isFinished = false;
//    public bool isRunning = false;
//    public IEnumerator Method;
//    string MethodName;
//    Events.UpdateCallback loop;

//    static bool isElapsed(float TimeStarted, float TimeToElapse)
//    {
//        if ((float)(Game.TotalElapsedGameTime - TimeStarted) >= TimeToElapse)
//            return true;
//        else
//            return false;
//    }

//    public Coroutine(IEnumerator _Function)
//    {
//        Method = _Function;
//        MethodName = getMethodName(_Function);
//    }

//    public void Run()
//    {
//        RoutinesList.Add(this);
//        this.isRunning = true;
//        this.isFinished = false;
//        loop = Events.UpdateCallback.Start(Loop, 0);
//    }

//    public void Stop()
//    {
//        RoutinesList.Remove(this);
//        this.isRunning = false;
//        loop.Stop();
//    }

//    void Loop(float elapsed)
//    {
//        if (Method.Current is WaitForSeconds)
//        {
//            WaitForSeconds x = (WaitForSeconds)Method.Current;
//            if (!isElapsed(x.CurrentTime, x.Seconds * 1000))
//                return;
//        }
//        if (!Method.MoveNext())
//        {
//            isRunning = false;
//            isFinished = true;
//            loop.Stop();
//        }
//    }

//    static private string getMethodName(IEnumerator Method)
//    {
//        string methodName = Method.ToString();
//        methodName = methodName.Split('<', '>')[1];
//        return methodName;
//    }

//    public static void Stop(IEnumerator MethodToStop)
//    {
//        int index = RoutinesList.FindIndex(element => element.Method == MethodToStop);
//        if (index != -1)
//            RoutinesList[index].Stop();
//    }

//    public static void Stop(string MethodToStop)
//    {
//        int index = RoutinesList.FindIndex(element => element.MethodName == MethodToStop);
//        if (index != -1)
//            RoutinesList[index].Stop();
//    }
//}

//class WaitForSeconds : GameScriptInterface
//{
//    public float Seconds;
//    public float CurrentTime;
//    public WaitForSeconds(float _Seconds)
//    {
//        CurrentTime = Game.TotalElapsedGameTime;
//        Seconds = _Seconds;
//    }
//}

//public void StopCoroutine(string MethodToStop)
//{
//    Coroutine.Stop(MethodToStop);
//    return;
//}

//public void StopCoroutine(IEnumerator MethodToStop)
//{
//    Coroutine.Stop(MethodToStop);
//    return;
//}

//public void StopCoroutine(Coroutine CoroutineToStop)
//{
//    Coroutine.Stop(CoroutineToStop.Method);
//    return;
//}

//public Coroutine StartCoroutine(IEnumerator MethodToStart)
//{
//    Coroutine rou = new Coroutine(MethodToStart);
//    rou.Run();
//    return rou;
//}
using System.Collections;
using UnityEngine;

public class CoRunner : MonoBehaviour
{
    public static void RunCoroutine(IEnumerator co)
    {
        var temp = new GameObject("CoRunner (TEMP)");
        DontDestroyOnLoad(temp);

        var runCo = temp.AddComponent<CoRunner>();

        runCo.StartCoroutine(runCo.MonitorRun(co));
    }

    private IEnumerator MonitorRun(IEnumerator co)
    {
        while (co.MoveNext())
        {
            yield return co.Current;
        }

        Destroy(this.gameObject);
    }
}
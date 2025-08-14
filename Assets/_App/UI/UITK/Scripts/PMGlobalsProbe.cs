using System.Collections;
using UnityEngine;
using HutongGames.PlayMaker;

public class PMGlobalsProbe : MonoBehaviour
{
    IEnumerator Start()
    {
        for (int i = 0; i < 6; i++)
        {
            var g = PlayMakerGlobals.Instance.Variables;
            var movie = g.GetFsmString("MovieName");
            var ver   = g.GetFsmString("Version");
            string mv = movie == null ? "<null var>" : (movie.Value ?? "<null value>");
            string vv = ver   == null ? "<null var>" : (ver.Value   ?? "<null value>");
            Debug.Log($"[PMGlobalsProbe] t={i}s MovieName={mv} | Version={vv}");
            yield return new WaitForSeconds(1f);
        }
    }
}
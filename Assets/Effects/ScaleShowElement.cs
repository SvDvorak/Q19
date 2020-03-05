using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Assets.Tutorial
{
    public class ScaleShowElement : TimedTask
    {
        public Transform Element;
        public AudioSource Sound;

        public void Start()
        {
            Element.localScale = Vector3.zero;
        }

        public override IEnumerator StartTask()
        {
            Sound?.Play();
            yield return Element.DOScale(1, 0.3f).WaitForCompletion();
        }
    }
}
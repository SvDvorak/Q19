using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Assets.Tutorial
{
    public class ScaleHideElement : TimedTask
    {
        public Transform Element;
        public AudioSource Sound;

        public override IEnumerator StartTask()
        {
            Sound?.Play();
            yield return Element.DOScale(0, 0.5f).WaitForCompletion();
        }
    }
}

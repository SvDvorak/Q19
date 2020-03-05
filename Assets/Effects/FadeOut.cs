using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Transitions
{
    public class FadeOut : TimedTask
    {
        public bool StartVisible = true;
        public Image Image;

        public void Start()
        {
            if(StartVisible)
                Image.DOFade(1, 0).Complete();
        }

        public override IEnumerator StartTask()
        {
            yield return Image
                .DOFade(0, 0.1f)
                .WaitForCompletion();
        }
    }
}
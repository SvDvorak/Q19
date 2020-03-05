using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Transitions
{
    public class FadeIn : TimedTask
    {
        public bool StartHidden = true;
        public Image Image;

        public void Start()
        {
            if(StartHidden)
                Image.DOFade(0, 0).Complete();
        }

        public override IEnumerator StartTask()
        {
            yield return Image
                .DOFade(1, 0.1f)
                .WaitForCompletion();
        }
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace Q19
{
    class ScoreTicker
    {
        public bool Active { get; set; } = false;
        public int Score => (int)_score;
        public bool Finished => Score == _end;
        public float TimeSinceLastChange { get; private set; } = float.MaxValue;
        public Color Color { get; set; } = Color.White;

        private float _score;
        private readonly float _runTime;
        private readonly int _delta;
        private readonly int _end;
        private readonly SoundEffectInstance _sound;

        public ScoreTicker(int start, int end, SoundEffectInstance sound, float runTime)
        {
            _score = start;
            _end = end;
            _delta = end - start;
            _sound = sound;
            _runTime = _delta == 0 ? 0 : runTime;
        }

        public void Update(float elapsedSeconds)
        {
            var previousScore = Score;
            if (Active)
            {
                _score = MathHelper.Min(_score + _delta * (elapsedSeconds / _runTime), _end);

                if (Finished)
                    Active = false;
            }


            if(previousScore != Score)
            {
                const float change = 0.05f;
                _sound.Pitch = Helpers.Rnd.Float(change) - change / 2;
                _sound.Volume = 0.6f + Helpers.Rnd.Float(change) - change / 2;
                _sound.Play();
                TimeSinceLastChange = 0;
            }
            else
            {
                TimeSinceLastChange += elapsedSeconds;
            }
        }
    }
}
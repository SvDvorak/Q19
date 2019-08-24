using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace Q19
{
    class MovementSounds
    {
        private SoundEffectInstance _currentMoveSound;
        private readonly List<SoundEffect> _movementSounds = new List<SoundEffect>();
        private double _walkSoundDelay;

        public MovementSounds(ContentManager content)
        {
            _movementSounds.Add(content.Load<SoundEffect>("Audio/Movement/Move1"));
            _movementSounds.Add(content.Load<SoundEffect>("Audio/Movement/Move2"));
            _movementSounds.Add(content.Load<SoundEffect>("Audio/Movement/Move3"));
            _movementSounds.Add(content.Load<SoundEffect>("Audio/Movement/Move4"));
            _movementSounds.Add(content.Load<SoundEffect>("Audio/Movement/Move5"));
            _currentMoveSound = _movementSounds[0].CreateInstance();
        }

        public void UpdateMovementSound(bool isMoving, GameTime gameTime)
        {
            if (isMoving)
            {
                if (_walkSoundDelay < 0)
                {
                    var maxPitch = 0.2f;
                    _currentMoveSound = _movementSounds[Helpers.Rnd.Next(_movementSounds.Count)].CreateInstance();
                    _currentMoveSound.Pitch = maxPitch / 2 - Helpers.Rnd.Float(maxPitch);
                    _currentMoveSound.Volume = 0.16f;
                    _currentMoveSound.Play();
                    _walkSoundDelay += 0.2;
                }
                else
                {
                    _walkSoundDelay -= gameTime.ElapsedGameTime.TotalSeconds;
                }
            }

            if (_currentMoveSound.State == SoundState.Playing && !isMoving)
            {
                _currentMoveSound.Stop(false);
            }
        }
    }
}
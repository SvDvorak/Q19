using System;
using System.Collections.Generic;
using System.Diagnostics;
using jsmars;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Q19
{
    class StartLevel : Level
    {
        private Rectangle _readyPlayersArea;
        private bool _startingGame;

        public StartLevel()
        {
            var hallway = new Rectangle(-192, 19, 386, 68);
            PlayBounds.Add(hallway);
            var exitDoor = new Door(this, new Vector2(140, 24), 2, p => _startingGame = true);
            exitDoor.Open(true);
            Add(exitDoor);

            var unusedEntryDoor = new Door(this, new Vector2(-160, 24), 2, p => { });
            unusedEntryDoor.Open(true);
            Add(unusedEntryDoor);
            Delay(1f, () => unusedEntryDoor.Close());


            _readyPlayersArea = new Rectangle(44, -43, 156, 140);

            Add(new Entity()
            {
                TexturePath = "World/startscreen",
                Depth = Depths.Background,
            });

            Add(new Light(180));
        }

        public override void OnEnter()
        {
            Q19Game.Instance.Scene.LoadPlayers();
            var players = Q19Game.Instance.Players;

            players.ForEach(x => x.IsActive = true);

            var entryPosition = new Vector2(-152, 4);
            var doorExitPoint = entryPosition + new Vector2(0, 30);
            players.SetAndMoveTo(0, entryPosition, doorExitPoint, new Vector2(-120, 35));
            players.SetAndMoveTo(1, entryPosition, doorExitPoint, new Vector2(-130, 75));
            players.SetAndMoveTo(2, entryPosition, doorExitPoint, new Vector2(-154, 39));
            players.SetAndMoveTo(3, entryPosition, doorExitPoint, new Vector2(-160, 71));

            Q19Game.Instance.Scene.TotalScore = 0;
            foreach (var player in players)
            {
                player.TotalScore = 0;
            }

            base.OnEnter();
        }

        public override bool IsComplete()
        {
            return _startingGame;
        }

        public override Level GetNext()
        {
            if(!Q19Game.DebugMode || Input.KeyDown(Keys.Space))
            {
                var toRemove = new List<Player>();
                Q19Game.Instance.Scene.Visit(e =>
                {
                    if (e is Player p && !_readyPlayersArea.Contains(p.Position))
                    {
                        toRemove.Add(p);
                    }
                });

                foreach (var player in toRemove)
                {
                    Q19Game.Instance.Scene.RemovePlayer(player);
                }
            }

            return new TransitionLevel(TransitionState.Current, new GameLevel(0));
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            if (Q19Game.Instance.DebugDrawing)
            {
                sb.Draw(Q19Game.Pixel, _readyPlayersArea, new Color(30, 30, 100, 50));
            }

            base.Draw(sb, gameTime);
        }
    }
}
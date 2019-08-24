using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using jsmars.Game2D;

namespace Q19
{
    class Relic : Entity
    {
        public Vector2 OriginalPosition { get; set; }
        public RelicInfo RelicInfo { get; private set; }
        public RelicVariant Variant { get; private set; }
        public string Comment { get; private set; } = "";

        Texture2D bgTexture;

        static List<RelicInfo> relics;
        public static List<string> genericComments;

        public Relic()
        {
            #region Relic List

            const int s = 32;
            Rectangle r(int x, int y, int w = 1, int h = 1) => new Rectangle(x * s, y * s, s * w, s * h);

            genericComments = new List<string>()
            {
                "Hey that was mine!",
                "Why did you do that",
                "You shouldn't burn other peoples things",
                "I was saving that!",
                "Now why did you go and do that?",
                "I wish you hadn't done that",
                "Now that's just mean",
                "Oh well, I didn't really need that anyway",
                "I already forgot what was hanging there",
            };

            if (relics == null)
            {
                relics = new List<RelicInfo>()
                {
                    new RelicInfo("Sword", r(0, 0, 2, 1))
                    {
                        new RelicVariant("Great Sword", 3, -5)
                        {
                            "That was a really good sword you know!",
                            "I didn't know that sword even could burn",
                        },
                        new RelicVariant("Fake Plastic Sword", 0.4f, 0)
                        {
                            "I was tired of looking at it anyway",
                            "I hope you didn't ingest the fumes",
                        },
                    },
                    new RelicInfo("Plant", r(2, 0))
                    {
                        new RelicVariant("Plant", 1, -1)
                        {
                            "Good with some spring cleaning I guess",
                            "I kindof liked that plant",
                        },
                        new RelicVariant("Grandmas favorite plant", 1, -10)
                        {
                            "Your grandma will be so sad when she sees that it's gone",
                            "She's kept that alive since before you were born!",
                        },
                    },
                    new RelicInfo("Chest", r(0, 1, 2))
                    {
                        new RelicVariant("Chest", 1, -1)
                        {
                            "You didn't even open to check what was in it first!?",
                            "I'm amazed you were able to carry that!"
                        },
                        new RelicVariant("Locked Chest", 2, -3)
                        {
                            "That chest was over 300 years old",
                            "I've never been able to open that chest, it was invaluable"
                        }
                    },
                    new RelicInfo("Barrel", r(0, 3))
                    {
                        new RelicVariant("Barrel", 1, 0)
                        {
                            "Your lucky you didn't take the one with nitro glycerin",
                            "It was just an old barrel",
                        },
                        new RelicVariant("Rum Barrel", 2, -3)
                        {
                            "That was my favorite rum you fool!",
                            "No! Not my rum!",
                        }
                    },
                    new RelicInfo("BarrelEmpty", r(1, 3))
                    {
                        new RelicVariant("BarrelEmpty", 1, -1)
                        {
                            "Such a waste of a good barrel",
                            "I was gonna fill that with whisky"
                        }
                    },
                    new RelicInfo("Axe", r(1, 2))
                    {
                        new RelicVariant("Axe", 1, -1)
                        {
                            "That was a perfectly good axe",
                            "How will I now defend myself?"
                        },
                        new RelicVariant("Epic Battle Axe", 2, -5)
                        {
                            "That was a legendary axe!",
                            "That axe was worth a fortune!"
                        },
                    },
                    new RelicInfo("Bag", r(0, 2))
                    {
                        new RelicVariant("Bag", 1, -1)
                        {
                            "You wouldn't believe how many items fit in that bag",
                            "That thing was full of groceries!"
                        }
                    },
                    new RelicInfo("Scroll", r(3, 0))
                    {
                        new RelicVariant("Scroll", 1, -1)
                        {
                            "That was a magical scroll",
                            "Oh... that was my medicine prescription"
                        },
                        new RelicVariant("Fire Scroll", 2, -1)
                        {
                            "That was a magical scroll",
                            "Oh... that was my medicine prescription"
                        }
                    },
                    new RelicInfo("Mirror", r(2, 1, 2))
                    {
                        new RelicVariant("Mirror", 1, -1)
                        {
                            "You should never break a mirror!",
                            "I probably feel better if I don't look at that every day"
                        }
                    },
                    new RelicInfo("RedFlag", r(4, 0))
                    {
                        new RelicVariant("RedFlag", 1, -1)
                        {
                            "Oh I liked that flag",
                            "I remember using that to flag down the Midway"
                        }
                    },
                    new RelicInfo("GreenFlag", r(4, 1))
                    {
                        new RelicVariant("RedFlag", 1, -1)
                        {
                            "That was worth a lot back in the days you know",
                            "I'll never see it flapping in the wind again"
                        }
                    },
                    new RelicInfo("Shelf1", r(5, 0))
                    {
                        new RelicVariant("Shelf1", 1, -1)
                        {
                            "Hey that was my favorite book",
                            "I wasn't finished reading that"
                        }
                    },
                    new RelicInfo("Shelf2", r(5, 1))
                    {
                        new RelicVariant("Shelf2", 1, -1)
                        {
                            "You never know when you need a mana potion",
                            "That thing was spoiled anyway"
                        }
                    },
                    new RelicInfo("Shelf3", r(5, 2))
                    {
                        new RelicVariant("Shelf3", 1, -1)
                        {
                            "Health potions aren't easy to find these days",
                            "You're removing all my shelfspace"
                        }
                    },
                    new RelicInfo("Shelf4", r(5, 3))
                    {
                        new RelicVariant("Shelf4", 1, -1)
                        {
                            "I was growing an avacado",
                            "Good thing there was nothing of value on that"
                        }
                    },
                    new RelicInfo("Paper1", r(6, 0))
                    {
                        new RelicVariant("Paper1", 1, -1)
                        {
                            "That was my favorite bow",
                            "Springy was what I called that bow"
                        }
                    },
                    new RelicInfo("Paper2", r(6, 1))
                    {
                        new RelicVariant("Paper2", 1, -1)
                        {
                            "That was a memoir of my first bomb",
                            "Good thing you burned thouse, just old love letters"
                        }
                    },
                    new RelicInfo("Paper3", r(6, 2))
                    {
                        new RelicVariant("Paper3", 1, -1)
                        {
                            "That net was so good at catching butterflies",
                            "That's ok, I don't think it's possible to read that anymore"
                        }
                    },
                    new RelicInfo("Paper4", r(6, 3))
                    {
                        new RelicVariant("Paper4", 1, -1)
                        {
                            "That was the last existing copy of that key",
                            "How will I now lock my house"
                        }
                    },
                };
            }

            #endregion

            TexturePath = "World/Sprites1";
            CanPickup = true;
            Depth = Depths.PuzzlePieceIncrement;
            RelicInfo = Helpers.Rnd.FromList(relics);
            Variant = Helpers.Rnd.FromList(RelicInfo);
            Comment = Variant.Count > 0 ? Helpers.Rnd.FromList(Variant) : "";
            SourceArea = RelicInfo.Source;
        }

        public override void LoadContent(ContentManager content)
        {
            bgTexture = content.Load<Texture2D>("World/Sprites1bg");
            base.LoadContent(content);
            OriginalPosition = Position;
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            sb.Draw(bgTexture, new Vector2((int)OriginalPosition.X, (int)OriginalPosition.Y), SourceArea, GetGameLevel().GameOver ? Color.Red : Color.White, drawRotation, Origin, Scale, Effects, Depth + 0.000001f);
            base.Draw(sb, gameTime);
        }

        public override void OnDropped(Player player)
        {
            if (GetGameLevel().TryBurn(this))
            {
                string c = Comment;
                int p = Variant.PointValue;
                if (Comment.Length == 0 || Helpers.Rnd.Float() < 0.3f)
                {
                    c = Helpers.Rnd.FromList(genericComments);
                    p = 0;
                }

                GetGameLevel().RelicBurned(c, p);
            }
            base.OnDropped(player);
        }
    }

    public class RelicInfo : List<RelicVariant> // variants in base list
    {
        public RelicInfo(string name, Rectangle source) { BaseName = name; Source = source; }
        public string BaseName { get; private set; }
        public Rectangle Source { get; set; }
    }

    public class RelicVariant : List<string> // comments in base list
    {
        public RelicVariant(string name, float fireValue, int pointValue) { Name = name; FireValue = fireValue; PointValue = pointValue; }
        public string Name { get; private set; }
        public float FireValue { get; private set; }
        public int PointValue { get; private set; }
    }
}

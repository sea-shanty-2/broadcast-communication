using J3QQ4;

namespace BroadcastCommunication
{
    public class Utility
    {
        public static Polarity EmojiToPolarity(string emoji)
        {
            switch (emoji)
            {
                case Emoji.Fire:
                case Emoji.Joy:
                case Emoji.Thumbsup:
                case Emoji.B:
                    return Polarity.Positive;
                case Emoji.Thumbsdown:
                case Emoji.Angry:
                    return Polarity.Negative;
        }
    }
}
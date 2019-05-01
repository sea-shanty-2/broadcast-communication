using System;

namespace BroadcastCommunication
{
    public class EmojiNotAllowedException : Exception
    {
        public string Emoji { get; }
        
        public EmojiNotAllowedException(string emoji)
        {
            Emoji = emoji;
        }
    }
}
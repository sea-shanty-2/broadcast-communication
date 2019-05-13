using System;

namespace BroadcastCommunication
{
    public class EmojiNotAllowedException : Exception
    {
        private string Emoji { get; }
        
        public EmojiNotAllowedException(string emoji)
        {
            Emoji = emoji;
        }
    }
}
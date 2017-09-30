﻿using Lynx.Database;
namespace Lynx.Models.Database
{
    public class ServerModel
    {
        public string Id { get; set; }
        public string ServerPrefix { get; set; }
        public EmbedB WelcomeMessage { get; set; } = new EmbedB();
        public EmbedB LeaveMessage { get; set; } = new EmbedB();
        public EventsWrapper Events { get; set; } = new EventsWrapper();
        public ModerationWrapper Moderation { get; set; } = new ModerationWrapper();
        public Orbs Orbs { get; set; } = new Orbs();
    }
}
    

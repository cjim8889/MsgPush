namespace TelePush.Backend.Core
{
    public enum DispatcherType
    {
        Unknown = 0,
        Text = 1,
        Photo = 2,
        Audio = 3,
        Video = 4,
        Voice = 5,
        Document = 6,
        Sticker = 7,
        Location = 8,
        Contact = 9,
        Venue = 10,
        Game = 11,
        VideoNote = 12,
        Invoice = 13,
        SuccessfulPayment = 14,
        WebsiteConnected = 15,
        ChatMembersAdded = 16,
        ChatMemberLeft = 17,
        ChatTitleChanged = 18,
        ChatPhotoChanged = 19,
        MessagePinned = 20,
        ChatPhotoDeleted = 21,
        GroupCreated = 22,
        SupergroupCreated = 23,
        ChannelCreated = 24,
        MigratedToSupergroup = 25,
        MigratedFromGroup = 26,
        Animation = 27,
        Any = 28,
        Reply = 29
    }
}
